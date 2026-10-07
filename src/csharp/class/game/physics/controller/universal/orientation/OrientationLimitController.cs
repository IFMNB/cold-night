using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Чей <c>Node3D.RotationOrder</c> задаёт порядок Euler-разложения.
/// </summary>
public enum OrientationOrderSource
{
    /// <summary>
    /// Порядок берётся у ограничиваемого узла (Target).
    /// </summary>
    Target,

    /// <summary>
    /// Порядок берётся у узла <see cref="OrientationAnchorController.AnchorTarget"/>.
    /// Если он не используется (AnchorMode = Preset) или невалиден, берётся порядок Target.
    /// </summary>
    AnchorTarget
}

/// <summary>
/// Контроллер, ограничивающий ориентацию объекта относительно якоря (см. <see cref="OrientationAnchorController"/>).
///
/// <para>
/// Отклонение цели от якоря раскладывается на Euler, и каждая ось (X = pitch, Y = yaw, Z = roll)
/// ограничивается независимо, поэтому правка одной оси не порождает смещений в других.
/// Оси с <c>IgnoreX/Y/Z = true</c> не ограничиваются.
/// </para>
///
/// <para>
/// Любое Euler-разложение имеет особую точку на средней оси порядка (±90°): рядом с ней остальные две
/// оси перескакивают на 180°. Поэтому средней нужно делать ту ось, которая никогда не подходит к 90°
/// (с учётом HardLimit): Yxz = pitch, Yzx и Xzy = roll, Xyz и Zyx = yaw, Zxy = pitch. Диапазон средней оси
/// принудительно ограничен <see cref="MiddleAxisCap"/>. Средняя ось обязана быть ограничиваемой
/// (не игнорироваться), иначе защиты от перескока нет.
/// </para>
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/orbit.svg")]
public partial class OrientationLimitController : OrientationAnchorController
{
    /// <summary>
    /// Максимум по модулю для средней оси разложения (градусы), чтобы не дойти до особой точки.
    /// </summary>
    private const float MiddleAxisCap = 89f;

    [Export] public PDCalculator YawCalculator { get; set; } = new();
    [Export] public PDCalculator PitchCalculator { get; set; } = new();
    [Export] public PDCalculator RollCalculator { get; set; } = new();

    /// <summary>
    /// Чей RotationOrder определяет порядок Euler-разложения.
    /// </summary>
    [Export] public OrientationOrderSource OrderSource { get; set; } = OrientationOrderSource.Target;

    private Vector2 _yawLimit = new(-180f, 180f);
    private Vector2 _pitchLimit = new(-180f, 180f);
    private Vector2 _rollLimit = new(-180f, 180f);
    private Vector2 _hardLimit = new(-30f, 30f);

    /// <summary>
    /// Min/max yaw (вокруг Y) от якоря, градусы.
    /// </summary>
    [Export] public Vector2 YawLimit { get => _yawLimit; set => _yawLimit = ClampRange(value, 180f); }

    /// <summary>
    /// Min/max pitch (вокруг X) от якоря, градусы.
    /// </summary>
    [Export] public Vector2 PitchLimit { get => _pitchLimit; set => _pitchLimit = ClampRange(value, 180f); }

    /// <summary>
    /// Min/max roll (вокруг Z) от якоря, градусы.
    /// </summary>
    [Export] public Vector2 RollLimit { get => _rollLimit; set => _rollLimit = ClampRange(value, 180f); }

    /// <summary>
    /// Допустимый выход за границу до жёсткого клампа: X добавляется к min, Y к max.
    /// Внутри этой зоны цель тянут обратно PD-калькуляторы, за ней она обрезается жёстко.
    /// </summary>
    [Export] public Vector2 HardLimit { get => _hardLimit; set => _hardLimit = ClampRange(value, 30f); }

    /// <summary>
    /// Порядок разложения: RotationOrder Target или узла-якоря (см. <see cref="OrientationOrderSource"/>).
    /// </summary>
    protected override EulerOrder ResolveOrder(Node3D target)
    {
        if (OrderSource == OrientationOrderSource.AnchorTarget
            && AnchorMode == OrientationAnchorMode.AnchorTarget
            && (AnchorTarget?.IsActive() ?? false))
        {
            return AnchorTarget!.RotationOrder;
        }

        return base.ResolveOrder(target);
    }

    protected override Quaternion Correct(Quaternion deviation, double delta)
    {
        if (IgnoresAll)
            return deviation;

        int middle = GetMiddleAxis(ResolveOrder(Target!));
        Vector3 euler = Decompose(deviation);

        return Compose(new Vector3(
            LimitAxis(euler.X, IgnoreX, _pitchLimit, PitchCalculator, middle == 0, delta),
            LimitAxis(euler.Y, IgnoreY, _yawLimit, YawCalculator, middle == 1, delta),
            LimitAxis(euler.Z, IgnoreZ, _rollLimit, RollCalculator, middle == 2, delta)));
    }

    protected override void OnStopped()
    {
        PitchCalculator.Reset();
        YawCalculator.Reset();
        RollCalculator.Reset();
    }

    /// <summary>
    /// Индекс средней оси порядка: 0 = X (pitch), 1 = Y (yaw), 2 = Z (roll).
    /// </summary>
    private static int GetMiddleAxis(EulerOrder order) => order switch
    {
        EulerOrder.Xyz or EulerOrder.Zyx => 1,
        EulerOrder.Xzy or EulerOrder.Yzx => 2,
        _ => 0 // Yxz, Zxy
    };

    /// <summary>
    /// PD-притяжение к допустимому диапазону, затем жёсткий кламп с учётом HardLimit.
    /// Угол и пределы в градусах относительно якоря. Игнорируемая ось возвращается как есть.
    /// Для средней оси разложения весь диапазон дополнительно режется по MiddleAxisCap.
    /// </summary>
    private float LimitAxis(float angle, bool ignore, Vector2 limit, PDCalculator calculator, bool isMiddle, double delta)
    {
        if (ignore)
            return angle;

        float cap = isMiddle ? MiddleAxisCap : 180f;

        float min = Mathf.Clamp(Mathf.Min(limit.X, limit.Y), -cap, cap);
        float max = Mathf.Clamp(Mathf.Max(limit.X, limit.Y), -cap, cap);

        float target = Mathf.Clamp(angle, min, max);
        float force = calculator.CalculateForce(target, angle, delta);
        angle += force * (float)delta;

        return Mathf.Clamp(
            angle,
            Mathf.Clamp(min + _hardLimit.X, -cap, cap),
            Mathf.Clamp(max + _hardLimit.Y, -cap, cap));
    }

    private static Vector2 ClampRange(Vector2 v, float range)
        => new(Mathf.Clamp(v.X, -range, range), Mathf.Clamp(v.Y, -range, range));
}
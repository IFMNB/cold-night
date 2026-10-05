using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>Откуда берётся якорь (центр пределов) для <see cref="OrientationLimitController"/>.</summary>
public enum OrientationAnchorMode
{
    /// <summary>Предустановка: фиксированная ориентация в базисе родителя (<see cref="OrientationLimitController.PresetRotationDegrees"/>).</summary>
    Preset,

    /// <summary>Якорем служит ориентация отдельного узла <see cref="OrientationLimitController.AnchorTarget"/>.</summary>
    AnchorTarget
}

/// <summary>Чей <c>Node3D.RotationOrder</c> задаёт порядок Euler-разложения.</summary>
public enum OrientationOrderSource
{
    /// <summary>Порядок берётся у ограничиваемого узла (Target).</summary>
    Target,

    /// <summary>
    /// Порядок берётся у узла <see cref="OrientationLimitController.AnchorTarget"/>.
    /// Если он не используется (AnchorMode = Preset) или невалиден, берётся порядок Target.
    /// </summary>
    AnchorTarget
}

/// <summary>
/// Контроллер, ограничивающий ориентацию объекта относительно родителя.
///
/// <para>
/// Отклонение цели от якоря считается кватернионом <c>Anchor⁻¹ · Target</c> и раскладывается на Euler
/// в порядке <c>Node3D.RotationOrder</c> выбранного узла (<see cref="OrderSource"/>). Каждая ось
/// (X = pitch, Y = yaw, Z = roll) ограничивается независимо, поэтому правка одной оси не порождает
/// смещений в других.
/// </para>
///
/// <para>
/// Любое Euler-разложение имеет особую точку на средней оси порядка (±90°): рядом с ней остальные две
/// оси перескакивают на 180°. Поэтому средней нужно делать ту ось, которая никогда не подходит к 90°
/// (с учётом HardLimit): Yxz = pitch, Yzx и Xzy = roll, Xyz и Zyx = yaw, Zxy = pitch. Диапазон средней оси
/// принудительно ограничен <see cref="MiddleAxisCap"/>. Средняя ось обязана быть ограничиваемой
/// (Limit* = true), иначе защиты от перескока нет.
/// </para>
///
/// <para>
/// Якорь задаётся либо предустановкой, либо узлом <see cref="AnchorTarget"/>. Во втором случае его мировая
/// ориентация переводится в базис родителя цели. Узел-якорь не должен быть самим Target или его потомком.
/// </para>
///
/// <para>
/// Target трогается только когда хотя бы одна ось реально изменилась.
/// </para>
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/orbit.svg")]
public partial class OrientationLimitController : Universal3DPhysicsController
{
    private const float Deg2Rad = Mathf.Pi / 180f;
    private const float Rad2Deg = 180f / Mathf.Pi;

    /// <summary>Минимальное изменение (градусы), ради которого стоит писать в Target.</summary>
    private const float WriteEpsilonDeg = 1e-4f;

    /// <summary>Максимум по модулю для средней оси разложения (градусы), чтобы не дойти до особой точки.</summary>
    private const float MiddleAxisCap = 89f;

    [Export] public bool LimitYaw { get; set; } = true;
    [Export] public bool LimitPitch { get; set; } = true;
    [Export] public bool LimitRoll { get; set; } = true;

    [Export] public PDCalculator YawCalculator { get; set; } = new();
    [Export] public PDCalculator PitchCalculator { get; set; } = new();
    [Export] public PDCalculator RollCalculator { get; set; } = new();

    [Export] public OrientationAnchorMode AnchorMode { get; set; } = OrientationAnchorMode.Preset;

    /// <summary>Узел, чья ориентация служит якорем. Используется при AnchorMode = AnchorTarget.</summary>
    [Export] public Node3D? AnchorTarget { get; set; }

    /// <summary>Чей RotationOrder определяет порядок Euler-разложения.</summary>
    [Export] public OrientationOrderSource OrderSource { get; set; } = OrientationOrderSource.Target;

    private Vector3 _presetRotationDegrees = Vector3.Zero;
    private Quaternion _preset = Quaternion.Identity;

    /// <summary>Предустановленный якорь в базисе родителя (как RotationDegrees в инспекторе, Euler YXZ). Используется при AnchorMode = Preset.</summary>
    [Export]
    public Vector3 PresetRotationDegrees
    {
        get => _presetRotationDegrees;
        set
        {
            _presetRotationDegrees = value;
            _preset = Quaternion.FromEuler(value * Deg2Rad).Normalized();
        }
    }

    private Vector2 _yawLimit = new(-180f, 180f);
    private Vector2 _pitchLimit = new(-180f, 180f);
    private Vector2 _rollLimit = new(-180f, 180f);
    private Vector2 _hardLimit = new(-30f, 30f);

    /// <summary>Min/max yaw (вокруг Y) от якоря, градусы.</summary>
    [Export] public Vector2 YawLimit { get => _yawLimit; set => _yawLimit = ClampRange(value, 180f); }

    /// <summary>Min/max pitch (вокруг X) от якоря, градусы.</summary>
    [Export] public Vector2 PitchLimit { get => _pitchLimit; set => _pitchLimit = ClampRange(value, 180f); }

    /// <summary>Min/max roll (вокруг Z) от якоря, градусы.</summary>
    [Export] public Vector2 RollLimit { get => _rollLimit; set => _rollLimit = ClampRange(value, 180f); }

    /// <summary>
    /// Допустимый выход за границу до жёсткого клампа: X добавляется к min, Y к max.
    /// Внутри этой зоны цель тянут обратно PD-калькуляторы, за ней она обрезается жёстко.
    /// </summary>
    [Export] public Vector2 HardLimit { get => _hardLimit; set => _hardLimit = ClampRange(value, 30f); }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!Enabled || !IsInstanceValid(Target) || !TryGetAnchor(Target!, out Quaternion anchor))
        {
            Active = false;
            return;
        }

        Active = true;

        EulerOrder order = ResolveOrder(Target!);
        int middle = GetMiddleAxis(order);

        // Отклонение от якоря: X = pitch, Y = yaw, Z = roll (градусы).
        Quaternion deviation = (anchor.Inverse() * Target!.Quaternion).Normalized();
        Vector3 euler = new Basis(deviation).GetEuler(order) * Rad2Deg;

        Vector3 limited = new(
            LimitAxis(euler.X, LimitPitch, _pitchLimit, PitchCalculator, middle == 0, delta),
            LimitAxis(euler.Y, LimitYaw, _yawLimit, YawCalculator, middle == 1, delta),
            LimitAxis(euler.Z, LimitRoll, _rollLimit, RollCalculator, middle == 2, delta)
        );

        // Ничего не изменилось: Target не трогаем.
        if ((limited - euler).LengthSquared() < WriteEpsilonDeg * WriteEpsilonDeg)
            return;

        Quaternion limitedDeviation = new(Basis.FromEuler(limited * Deg2Rad, order));
        Target.Quaternion = (anchor * limitedDeviation).Normalized();
    }

    /// <summary>Порядок разложения: RotationOrder Target или узла-якоря (см. <see cref="OrientationOrderSource"/>).</summary>
    private EulerOrder ResolveOrder(Node3D target)
    {
        if (OrderSource == OrientationOrderSource.AnchorTarget
            && AnchorMode == OrientationAnchorMode.AnchorTarget
            && IsInstanceValid(AnchorTarget))
        {
            return AnchorTarget!.RotationOrder;
        }

        return target.RotationOrder;
    }

    /// <summary>Индекс средней оси порядка: 0 = X (pitch), 1 = Y (yaw), 2 = Z (roll).</summary>
    private static int GetMiddleAxis(EulerOrder order) => order switch
    {
        EulerOrder.Xyz or EulerOrder.Zyx => 1,
        EulerOrder.Xzy or EulerOrder.Yzx => 2,
        _ => 0 // Yxz, Zxy
    };

    /// <summary>
    /// Якорь в локальном базисе родителя Target. Для AnchorTarget его мировая ориентация
    /// переводится в базис родителя. Если узел-якорь не задан или невалиден, лимитер ничего не делает.
    /// </summary>
    private bool TryGetAnchor(Node3D target, out Quaternion anchor)
    {
        if (AnchorMode == OrientationAnchorMode.Preset)
        {
            anchor = _preset;
            return true;
        }

        if (!IsInstanceValid(AnchorTarget))
        {
            anchor = Quaternion.Identity;
            return false;
        }

        Quaternion anchorGlobal = AnchorTarget!.GlobalTransform.Basis.GetRotationQuaternion();
        Quaternion parentGlobal = target.GetParentNode3D() is { } parent
            ? parent.GlobalTransform.Basis.GetRotationQuaternion()
            : Quaternion.Identity;

        anchor = (parentGlobal.Inverse() * anchorGlobal).Normalized();
        return true;
    }

    /// <summary>
    /// PD-притяжение к допустимому диапазону, затем жёсткий кламп с учётом HardLimit.
    /// Угол и пределы в градусах относительно якоря. Выключенная ось возвращается как есть.
    /// Для средней оси разложения весь диапазон дополнительно режется по MiddleAxisCap.
    /// </summary>
    private float LimitAxis(float angle, bool enabled, Vector2 limit, PDCalculator calculator, bool isMiddle, double delta)
    {
        if (!enabled)
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
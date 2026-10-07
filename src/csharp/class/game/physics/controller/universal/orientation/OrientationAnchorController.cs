using Godot;

namespace ColdNight.src.game.physics;

/// <summary>Откуда берётся якорь (центр пределов) для <see cref="OrientationLimitController"/>.</summary>
public enum OrientationAnchorMode
{
    /// <summary>
    /// Предустановка: фиксированная ориентация в базисе родителя (<see cref="OrientationLimitController.PresetRotationDegrees"/>).
    /// </summary>
    Preset,

    /// <summary>
    /// Якорем служит ориентация отдельного узла <see cref="OrientationLimitController.AnchorTarget"/>.
    /// </summary>
    AnchorTarget
}


/// <summary>
/// Основа для контроллеров, ведущих ориентацию <see cref="Universal3DPhysicsController.Target"/> к якорю.
///
/// <para>
/// Отклонение цели от якоря считается кватернионом <c>Anchor⁻¹ · Target</c>. Наследник возвращает
/// исправленное отклонение (идеал — <see cref="Quaternion.Identity"/>, то есть цель совпала с якорем).
/// </para>
/// <para>
/// Euler-разложение идёт в <c>Node3D.RotationOrder</c> цели. У любого порядка есть особая точка на
/// средней оси (±90°), рядом с ней две другие оси перескакивают. Выбирайте порядок так, чтобы средняя ось
/// никогда не подходила к 90° (Yxz — pitch, Yzx и Xzy — roll, Xyz и Zyx — yaw, Zxy — pitch).
/// </para>
/// <para>
/// Якорь не должен быть самим Target или его потомком: такой якорь крутился бы вместе с целью.
/// </para>
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/orbit.svg")] public abstract partial class OrientationAnchorController : Universal3DPhysicsController
{
    protected const float Deg2Rad = Mathf.Pi / 180f;
    protected const float Rad2Deg = 180f / Mathf.Pi;

    /// <summary>
    /// Минимальное изменение (радианы), ради которого стоит писать в Target.
    /// </summary>
    private const float WriteEpsilonRad = 1e-6f;

    /// <summary>Не влиять на Pitch (вращение вокруг X).</summary>
    [Export] public bool IgnoreX { get; set; } = false;

    /// <summary>Не влиять на Yaw (вращение вокруг Y).</summary>
    [Export] public bool IgnoreY { get; set; } = false;

    /// <summary>Не влиять на Roll (вращение вокруг Z).</summary>
    [Export] public bool IgnoreZ { get; set; } = false;

    [Export] public OrientationAnchorMode AnchorMode { get; set; } = OrientationAnchorMode.Preset;

    /// <summary>
    /// Узел, чья ориентация служит якорем. Используется при AnchorMode = AnchorTarget.
    /// </summary>
    [Export] public Node3D? AnchorTarget { get; set; }

    private Vector3 _presetRotationDegrees = Vector3.Zero;
    private Quaternion _preset = Quaternion.Identity;

    /// <summary>
    /// Предустановленный якорь в базисе родителя (как RotationDegrees в инспекторе, Euler YXZ).
    /// Используется при AnchorMode = Preset. Для моделей с перевёрнутой после импорта осью Z
    /// выставьте Y = -180.
    /// </summary>
    [Export] public Vector3 PresetRotationDegrees
    {
        get => _presetRotationDegrees;
        set
        {
            _presetRotationDegrees = value;
            _preset = Quaternion.FromEuler(value * Deg2Rad).Normalized();
        }
    }

    protected bool IgnoresAll => IgnoreX && IgnoreY && IgnoreZ;
    protected bool IgnoresNone => !IgnoreX && !IgnoreY && !IgnoreZ;

    private bool _wasActive;

    /// <summary>
    /// Возвращает исправленное отклонение цели от якоря.
    /// </summary>
    protected abstract Quaternion Correct(Quaternion deviation, double delta);

    /// <summary>
    /// Вызывается один раз при переходе из рабочего состояния в нерабочее. Здесь сбрасывают
    /// внутреннее состояние калькуляторов.
    /// </summary>
    protected virtual void OnStopped() { }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!Enabled || Cancel(this, out var anchor))
        {
            Active = false;
            if (_wasActive)
            {
                _wasActive = false;
                OnStopped();
            }
            return;
        }

        Active = true;
        _wasActive = true;

        Quaternion deviation = (anchor.Inverse() * Target!.Quaternion).Normalized();
        Quaternion corrected = Correct(deviation, delta).Normalized();

        if (deviation.AngleTo(corrected) < WriteEpsilonRad)
            return;

        Target.Quaternion = (anchor * corrected).Normalized();
    }

    private static bool Cancel(OrientationAnchorController node, out Quaternion anchor)
    {
        anchor = default;

        if (!(node.Target?.IsActive() ?? false))
            return true;

        if (!node.TryGetAnchor(node.Target, out Quaternion result))
            return true;
        else
            anchor = result;

        if (!(node.AnchorTarget?.IsActive() ?? false))
            return true;

        if (node.Target.IsAncestorOf(node.AnchorTarget))
        {
            #if DEBUG
            GD.PushWarning("Cannot work with that AnchorTarget, because he is a child of Target");
            #endif

            return true;
        }

        return false;
    }

    /// <summary>
    /// Euler (градусы) в порядке RotationOrder цели: X = pitch, Y = yaw, Z = roll.
    /// </summary>
    protected Vector3 Decompose(Quaternion q)
        => new Basis(q).GetEuler(Target!.RotationOrder) * Rad2Deg;

    /// <summary>
    /// Порядок Euler-разложения. По умолчанию — RotationOrder цели; наследник может заменить.
    /// </summary>
    protected virtual EulerOrder ResolveOrder(Node3D target) => target.RotationOrder;

    /// <summary>Обратная операция к <see cref="Decompose"/>.</summary>
    protected Quaternion Compose(Vector3 eulerDegrees)
        => Basis.FromEuler(eulerDegrees * Deg2Rad, ResolveOrder(Target!)).GetRotationQuaternion();

    /// <summary>
    /// Якорь в локальном базисе родителя цели. Для AnchorTarget его мировая ориентация переводится
    /// в базис родителя. Если якорь не задан, невалиден или является целью/её потомком, контроллер не работает.
    /// </summary>
    private bool TryGetAnchor(Node3D target, out Quaternion anchor)
    {
        if (AnchorMode == OrientationAnchorMode.Preset)
        {
            anchor = _preset;
            return true;
        }

        if ((AnchorTarget?.IsActive() ?? false) == false || AnchorTarget == target)
        {
            anchor = Quaternion.Identity;
            return false;
        }

        Quaternion anchorGlobal = AnchorTarget.GlobalTransform.Basis.GetRotationQuaternion();
        Quaternion parentGlobal = target.GetFirstAncestor<Node3D>() is Node3D parent
            ? parent.GlobalTransform.Basis.GetRotationQuaternion()
            : Quaternion.Identity;

        anchor = (parentGlobal.Inverse() * anchorGlobal).Normalized();
        return true;
    }
}
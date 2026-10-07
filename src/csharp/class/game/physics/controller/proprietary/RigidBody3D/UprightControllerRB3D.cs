using Godot;
using ColdNight.src.common;

namespace ColdNight.src.game.physics;

public enum UprightAnchorMode
{
    /// <summary>Целевой Up берётся из ориентации узла <see cref="UprightController.AnchorTarget"/>.</summary>
    AnchorTarget,
    /// <summary>Целевой Up берётся из явно заданного поворота <see cref="UprightController.PresetRotationDegrees"/>.</summary>
    Preset
}

/// <summary>
/// Выравнивает Up «упавшего» или «падающего» <see cref="RigidBody3D"/> на целевой Up вектор.
/// Применяет момент по кратчайшему повороту, поэтому вращение вокруг самого Up (yaw) не затрагивается.
/// <see cref="PhysicsController.MaxForce"/> здесь ограничивает модуль момента.
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/arrow_up_to_line.svg")] public partial class UprightControllerRB3D : PhysicsControllerRB3D
{
    [ExportGroup("Anchor")]
    [Export] public UprightAnchorMode Mode { get; set; } = UprightAnchorMode.Preset;

    /// <summary>
    /// Нужен в режиме AnchorTarget. Не должен совпадать с Target.
    /// </summary>
    [Export] public Node3D? AnchorTarget { get; set; }

    /// <summary>
    /// Режим Preset: мировой поворот (Euler, градусы), Up которого и есть цель.
    /// </summary>
    [Export] public Vector3 PresetRotationDegrees { get; set; } = Vector3.Zero;

    [ExportGroup("Alignment")]
    /// <summary>
    /// Какая локальная ось тела считается его «Up» (для моделей с перевёрнутой конвенцией).
    /// </summary>
    [Export] public Vector3 LocalUp { get; set; } = Vector3.Up;

    [Export(PropertyHint.Range, "0,90,0.1,degrees")]
    public float DeadZoneDegrees { get; set; } = 0.5f;

    [ExportGroup("Regulator")]
    /// <summary>
    /// Шаблон (P / PD / PID). Для каждой мировой оси создаётся своя копия со своим состоянием.
    /// </summary>
    [Export] public FCalculator? Calculator
    {
        get => _calculator;
        set { _calculator = value; BuildCalculators(); }
    }

    private FCalculator? _calculator = new PDCalculator();
    private FCalculator? _x, _y, _z;
    private bool _warned;

    public override void _Ready() => BuildCalculators();

    private void BuildCalculators()
    {
        _x = _calculator?.Duplicate() as FCalculator;
        _y = _calculator?.Duplicate() as FCalculator;
        _z = _calculator?.Duplicate() as FCalculator;
    }

    public override void _PhysicsProcess(double delta)
    {
        var body = Target;
        if (!Enabled || (body?.IsActive() ?? false) == false || _x == null || _y == null || _z == null)
        {
            Deactivate();
            return;
        }
        if (!TryGetDesiredUp(body, out var desiredUp))
        {
            Deactivate();
            return;
        }

        var currentUp = (body.GlobalTransform.Basis * LocalUp).Normalized();
        var error = GetAlignmentError(body, currentUp, desiredUp);

        if (error.Length() < Mathf.DegToRad(DeadZoneDegrees) && body.AngularVelocity.LengthSquared() < 1e-4f)
        {
            Deactivate();
            return;
        }

        var torque = new Vector3(
            _x.CalculateForce(error.X, 0f, delta),
            _y.CalculateForce(error.Y, 0f, delta),
            _z.CalculateForce(error.Z, 0f, delta));

        if (Inverse) torque = -torque;
        if (MaxForce > 0f && torque.Length() > MaxForce)
            torque = torque.Normalized() * MaxForce;

        body.ApplyTorque(torque);
        Active = true;
    }

    private void Deactivate()
    {
        if (!Active) return;
        _x?.Reset(); _y?.Reset(); _z?.Reset();
        Active = false;
    }

    private bool TryGetDesiredUp(RigidBody3D body, out Vector3 up)
    {
        if (Mode == UprightAnchorMode.AnchorTarget)
        {
            if (AnchorTarget == null || ReferenceEquals(AnchorTarget, body))
            {
                if (!_warned)
                {
                    GD.PushWarning($"{Name}: в режиме AnchorTarget нужен AnchorTarget, отличный от Target.");
                    _warned = true;
                }
                up = default;
                return false;
            }
            _warned = false;
            up = AnchorTarget.GlobalTransform.Basis.Y.Normalized();
            return true;
        }

        var rad = new Vector3(
            Mathf.DegToRad(PresetRotationDegrees.X),
            Mathf.DegToRad(PresetRotationDegrees.Y),
            Mathf.DegToRad(PresetRotationDegrees.Z));
        up = Basis.FromEuler(rad).Y.Normalized();
        return true;
    }

    /// <summary>
    /// Ось кратчайшего поворота current→desired, длина вектора = угол в радианах.
    /// </summary>
    private static Vector3 GetAlignmentError(RigidBody3D body, Vector3 current, Vector3 desired)
    {
        float dot = Mathf.Clamp(current.Dot(desired), -1f, 1f);
        var axis = current.Cross(desired);

        if (axis.LengthSquared() < 1e-8f)
        {
            if (dot > 0f) return Vector3.Zero;

            var basis = body.GlobalTransform.Basis;
            axis = current.Cross(basis.X);
            if (axis.LengthSquared() < 1e-8f) axis = current.Cross(basis.Z);
        }

        return axis.Normalized() * Mathf.Acos(dot);
    }
}
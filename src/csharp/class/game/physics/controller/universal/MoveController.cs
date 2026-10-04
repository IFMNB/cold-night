using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Универсальный контроллер движения, использующий P-регулятор для достижения
/// заданной скорости объекта.
/// 
/// Состояние линейной скорости хранится в metadata цели, поэтому несколько
/// контроллеров могут работать с одним Node3D и совместно изменять его скорость.
/// </summary>
[GlobalClass]
public partial class MoveController : Universal3DPhysicsController, ICalculatedP, IWireReceiver
{
    [Export]
    public PCalculator Calculator { get; set; } = new();

    [Export]
    public WireIn? Input { get; set; }

    /// <summary>
    /// Направление движения.
    /// Нормализация выполняется автоматически.
    /// </summary>
    [Export]
    public Vector3 Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            NormalizedDirection = value.Normalized();
        }
    }

    /// <summary>
    /// Учитывать локальную систему координат цели.
    /// </summary>
    [Export]
    public bool Local { get; set; } = true;

    /// <summary>
    /// Нормализованное направление движения.
    /// </summary>
    public Vector3 NormalizedDirection { get; protected set; } = Vector3.Zero;

    /// <summary>
    /// Максимальная целевая скорость.
    /// </summary>
    protected virtual float TargetSpeed => MaxForce;

    /// <summary>
    /// Текущая линейная скорость цели.
    /// Скорость хранится непосредственно в metadata Target.
    /// </summary>
    protected Vector3 Velocity
    {
        get => GetVelocity();
        set => SetVelocity(value);
    }

    private Vector3 _direction = Vector3.Zero;

    /// <summary>
    /// Получить линейную скорость цели.
    /// Если metadata отсутствует, скорость считается нулевой.
    /// </summary>
    protected Vector3 GetVelocity()
    {
        if (Target is null || !Target.HasMeta(VelocityMetadata))
            return Vector3.Zero;

        return (Vector3)Target.GetMeta(VelocityMetadata);
    }

    /// <summary>
    /// Установить линейную скорость цели.
    /// </summary>
    protected void SetVelocity(Vector3 velocity)
    {
        if (Target is not null)
            Target.SetMeta(VelocityMetadata, velocity);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!Enabled ||
            Target is null ||
            !IsInstanceValid(Target) ||
            !Target.IsInsideTree())
        {
            Active = false;
            return;
        }

        Active = true;

        var direction = NormalizedDirection;

        if (Local)
            direction = Target.GlobalTransform.Basis * direction;

        var velocity = Velocity;
        var currentSpeed = velocity.Dot(direction);

        var force = direction *
            Calculator.CalculateForce(
                TargetSpeed,
                currentSpeed,
                delta
            );

        if (force.LengthSquared() > MaxForce * MaxForce)
            force = force.Normalized() * MaxForce;

        if (Inverse)
            force = -force;

        velocity += force * (float)delta;

        Velocity = velocity;

        Target.GlobalPosition += velocity * (float)delta;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input is not null)
        {
            if (VariantExtension.TryApply<Vector3>(
                (Variant)Direction,
                Input.Value,
                Input.Mode,
                out var result))
            {
                Direction = result;
            }
        }
    }
}
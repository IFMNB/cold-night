using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Контроллер, реализующий демпфирование линейного движения цели.
/// 
/// <para>
/// Контроллер читает текущую линейную скорость цели из metadata
/// <c>velocity</c> и применяет P-регулятор, стремящийся привести её
/// к нулевой скорости.
/// </para>
/// 
/// <para>
/// Скорость является общим состоянием цели, поэтому несколько
/// контроллеров могут одновременно изменять её.
/// </para>
/// </summary>
[GlobalClass]
public partial class DamperController : Universal3DPhysicsController, ICalculatedP
{

    [Export]
    public PCalculator Calculator { get; set; } = new();

    /// <summary>
    /// Минимальная скорость, при которой демпфер обновляет направление.
    /// </summary>
    [Export]
    public float MinSpeed { get; set; } = 0.01f;

    /// <summary>
    /// Для демпфера целевая скорость всегда равна нулю.
    /// </summary>
    protected virtual float TargetSpeed => 0f;

    protected Vector3 Velocity
    {
        get
        {
            if (Target is null || !Target.HasMeta(VelocityMetadata))
                return Vector3.Zero;

            return (Vector3)Target.GetMeta(VelocityMetadata);
        }
        set
        {
            if (Target is not null)
                Target.SetMeta(VelocityMetadata, value);
        }
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

        var velocity = Velocity;

        if (velocity.LengthSquared() <= MinSpeed * MinSpeed)
        {
            Active = false;
            return;
        }

        Active = true;

        var direction = velocity.Normalized();
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

        Velocity = velocity + force * (float)delta;
    }
}
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Контроллер демпфирования линейной скорости <see cref="RigidBody3D"/>.
/// <para>
/// Прикладывает центральную силу вдоль текущей линейной скорости цели (в глобальных координатах),
/// стремясь свести её модуль к нулю. Сила считается через <see cref="PDCalculator"/>:
/// целевая скорость 0, текущая — модуль скорости цели.
/// </para>
/// <para>
/// При скорости ниже <see cref="MinSpeed"/> воздействие не применяется, а состояние
/// калькулятора сбрасывается.
/// </para>
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/stop_sign.svg")]
public partial class DamperControllerRB3D : PhysicsControllerRB3D, ICalculatedPD
{
    [Export] public PDCalculator Calculator { get; set; } = new();

    /// <summary>
    /// Минимальная скорость цели, начиная с которой демпфер работает.
    /// </summary>
    [Export] public float MinSpeed { get; set; } = 0.01f;

    private bool _wasApplying;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!Enabled || Target is not { } target || !target.IsActive())
        {
            Stop();
            return;
        }

        var velocity = target.LinearVelocity;
        var speed = velocity.Length();

        if (speed < MinSpeed)
        {
            Stop();
            return;
        }

        var direction = velocity / speed;
        var force = (direction * Calculator.CalculateForce(0f, speed, delta)).LimitLength(MaxForce);

        target.ApplyCentralForce(Inverse ? -force : force);

        Active = true;
        _wasApplying = true;
    }

    private void Stop()
    {
        Active = false;

        if (!_wasApplying)
            return;

        Calculator.Reset();
        _wasApplying = false;
    }
}
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Объект, представляющий что-то схожее с объектами, у которых суть применять постоянный `VectorForce`
/// <para>
/// Основная разница между ними в том, что этот объект дополнительно учитывает текущую скорость и
/// реализует саму силу через `PD` калькулятор для консультации нормального ускорения
/// </para>
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/motion_vector.svg")]
public partial class RigidBody3DMoveController : RigidBody3DPhysicsController, ICalculatedPD
{
    [Export] public PDCalculator Calculator { get; set; } = new();

    [Export] public Wire? DirectionInput { get; set; }

    /// <summary>
    /// У этого контроллера направлением движения выступает этот вектор. Не важно, нормализован он или
    /// нет, контроллер всегда нормализует его самостоятельно и будет использовать релевантное поле
    /// <see cref="NormalizedDirection"/>
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
    /// Направление будет учитываться вместе с локальными трансформами цели.
    /// </summary>
    [Export] public bool Local { get; set; } = true;

    /// <summary>
    /// Нормализованное направление для контроллера, определяющее куда он сейчас будет двигаться
    /// 
    /// Контроллер сам устанавливает его
    /// </summary>
    public Vector3 NormalizedDirection { get; protected set; } = Vector3.Zero;

    protected virtual float TargetSpeed => MaxForce;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Enabled && (Target?.IsActive() ?? false))
        {
            Active = true;

            var direction = NormalizedDirection;

            // Переводим локальное направление цели в мировое пространство.
            if (Local)
                direction = Target.GlobalTransform.Basis.Orthonormalized() * direction;

            var linear_cur = Target.LinearVelocity.Dot(direction);
            var force = direction * Calculator.CalculateForce(TargetSpeed, linear_cur, delta);

            if (force.Length() > MaxForce)
                force = force.Normalized() * MaxForce;

            Target.ApplyCentralForce(Inverse ? -force : force);
            return;
        }

        Active = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (DirectionInput?.IsActive() ?? false)
        {
            Variant InputReceived = DirectionInput.GetReceivedEverPos(0).Mutate<Vector3>();
            if (VariantExtension.TryApply<Vector3>(Direction, InputReceived, DirectionInput.ReceiverMode, out var result))
                Direction = result;
        }
    }

    private Vector3 _direction = Vector3.Zero;
}
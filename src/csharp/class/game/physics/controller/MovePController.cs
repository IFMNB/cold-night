using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Объект, представляющий что-то схожее с объектами, у которых суть применять постоянный `VectorForce`
///
/// Основная разница между ними в том, что этот объект дополнительно учитывает текущую скорость и
/// реализует саму силу через `PD` калькулятор для консультации нормального ускорения
/// </summary>
[GlobalClass] public partial class MovePController : PhysicsController, ICalculatedPD
{
    [Export] public PDCalculator Calculator {get; set;} = new();
    
    /// <summary>
    /// У этого контроллера направлением движения выступает этот вектор. Не важно, нормализован он или
    /// нет, контроллер всегда нормализует его самостоятельно
    /// </summary>
    [Export] public Vector3 Direction {get => _direction;set
        {
            _direction = value;
            _normalized_direction = value.Normalized();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        if (IsInstanceValid(Target))
            if (Target!.IsInsideTree())
            {
                var linear_cur = Target!.LinearVelocity.Dot(_normalized_direction);
                Target!.ApplyCentralForce(Direction * Calculator.CalculateForce(MaxForce, linear_cur, delta));
            }
    }

    private Vector3 _direction = Vector3.Zero;
    private Vector3 _normalized_direction = Vector3.Zero;
}
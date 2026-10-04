using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;


/// <summary>
/// Контроллер который применяет или постоянную силу, или переменную силу для поворотов объекта. 
/// </summary>
[GlobalClass] public partial class OrientationController : Universal3DPhysicsController, IWireReceiver
{
    [Export] public WireIn? Input {get;set;}

    [Export] public Vector3 Direction {get => RealDirection;set
        {
            RealDirection = value;
            NormalizedDirection = value.Normalized();
        }
    }
    [Export] public Vector3 NormalizedDirection {get; private set;} = Vector3.Zero;

    /// <summary>
    /// Если в течение этого времени не будет входных данных, направление вращения будет сброшено в ноль.
    /// Применяется только если существует входной сигнал.
    /// </summary>
    [Export] public bool ResetDirectionAfterNoInput {get;set;} = true;

    /// <summary>
    /// Если в течение этого времени не будет входных данных, направление вращения будет сброшено в ноль.
    /// 
    /// Это касается только если существует входной сигнал и разрешено <see cref="ResetDirectionAfterNoInput"/>.
    /// Если входной сигнал отсутствует, направление вращения не сбрасывается.
    /// </summary>
    [Export] public double ResetAfterNoInputTime {get;set;} = 0.1;
    private double time_since_last_input = 0.0;

    protected Vector3 RealDirection {get;set;} = Vector3.Zero;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (IsInstanceValid(Input))
            if (Input.IsInsideTree())
                if(VariantExtension.TryApply<Vector3>((Variant)Direction, Input.Value, Input.Mode, out var result))
                {
                    Direction = result;
                    Input.Value = Vector3.Zero;
                    time_since_last_input = 0.0;
                }
    }

    public override void _PhysicsProcess (double delta)
    {
        if (Enabled)
            if (IsInstanceValid(Target))
                if (Target.IsInsideTree())
                {
                    if (IsInstanceValid(Input))
                        if (Input.IsInsideTree())
                            if (ResetDirectionAfterNoInput)
                            {
                                time_since_last_input += delta;

                                if (time_since_last_input > ResetAfterNoInputTime)
                                    Direction = Vector3.Zero;
                            }
                            else
                                time_since_last_input = 0.0;

                    Active = true;
                    Vector3 rotationDirection = Inverse ? -Direction : Direction;
                    Target.RotationDegrees += rotationDirection * MaxForce * (float)delta;     
                    
                    return;           
                }
                else
                    time_since_last_input = 0.0;
            else
                time_since_last_input = 0.0;

        Active = false;
    }
}
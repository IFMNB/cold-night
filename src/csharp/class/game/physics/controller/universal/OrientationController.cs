using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;


/// <summary>
/// Контроллер который применяет постоянную или переменную силу для поворотов объекта. 
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/rotate.svg")] public partial class OrientationController : Universal3DPhysicsController
{
    [Export] public Wire? Input {get;set;}

    /// <summary>
    /// В частном случае это свойство заставляет отменять влияние на Pitch, он же тангаж
    /// </summary>
    [Export] public bool IgnoreX {get;set;} = false;
    
    /// <summary>
    /// В частном случае это свойство заставляет отменять влияние на Yaw, он же рысканье
    /// </summary>
    [Export] public bool IgnoreY {get;set;} = false;

    /// <summary>
    /// В частном случае это свойство заставляет отменять влияние на Roll, он же крен
    /// </summary>
    [Export] public bool IgnoreZ {get;set;} = false;

    [Export] public Vector3 Direction {get => RealDirection;set => RealDirection = value;}

    protected Vector3 RealDirection {get;set;} = Vector3.Zero;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input?.IsActive() ?? false)
            if (VariantExtension.TryApply<Vector3>(Direction, Input.GetReceivedEverPos(0), Input.ReceiverMode, out var result))
                Direction = result;
                
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Enabled)
            if (IsInstanceValid(Target) && Target.IsInsideTree())
                {
                    Active = true;

                    Vector3 rotationDirection = Inverse ? -Direction : Direction;
                    Vector3 ignoreMask = new(
                        IgnoreX ? 0.0f : 1.0f,
                        IgnoreY ? 0.0f : 1.0f,
                        IgnoreZ ? 0.0f : 1.0f
                    );

                    rotationDirection *= ignoreMask;
                    Target.RotationDegrees += rotationDirection * MaxForce * (float)delta;

                    return;
                }

        Active = false;
    }
}
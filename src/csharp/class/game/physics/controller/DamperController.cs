using Godot;

namespace ColdNight.src.game.physics;

[Tool, GlobalClass] public partial class DamperController : MovePController
{
    [Export] public float MinSpeed { get; set; } = 0.01f;
    protected override float TargetSpeed => 0f;
    
    public override void _PhysicsProcess(double delta)
    {
        if (TryGetTarget(out var t) && t!.IsInsideTree() && t.LinearVelocity.LengthSquared() > MinSpeed * MinSpeed)
            Direction = t.LinearVelocity;
        
        base._PhysicsProcess(delta);
    }
}
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.input;

[GlobalClass] public partial class InputLookDirection : Node, ISwitchable, IWireSource
{
    [Export] public bool Enabled {get;set;} = true;
    [Export] public WireOut? Output {get;set;}
    [Export] public Vector2 Delta {get;set;} = Vector2.Zero;
    [Export] public Vector2 LookVelocity {get;set;} = Vector2.Zero;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Enabled)
            return;

        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            return;

        if (@event is InputEventMouseMotion motion)
        {
            Delta = motion.Relative;
            LookVelocity = motion.Velocity;
        } else if (@event is InputEventJoypadMotion joypadMotion)
        {
            GD.PushError("cannot handle joypad right now, unsupported + TODO");
        }
    }
}
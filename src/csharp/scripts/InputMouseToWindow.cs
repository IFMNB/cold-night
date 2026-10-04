using Godot;

namespace ColdNight.src.game.scripts;

public partial class InputMouseToWindow : Node
{
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event.IsReleased())
            return;

        if (@event is InputEventKey key)
            if (key.Keycode == Key.F3)
                if (Input.MouseMode == Input.MouseModeEnum.Captured)
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                else
                    Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
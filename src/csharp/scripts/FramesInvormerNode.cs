using Godot;

namespace ColdNight.src.game.scripts;

[Icon("res://addons/at-icons/node/info.svg")] public partial class FramesInvormerNode : Node
{
    [Export] public Label? AverageFPS {get;set;}
    [Export] public Label? TotalFramesPhysics {get;set;}
    [Export] public Label? TotalFramesProcess {get;set;}
    [Export] public Label? MaxFPS {get;set;}

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (AverageFPS is not null)
            AverageFPS.Text = Engine.GetFramesPerSecond().ToString();

        if (TotalFramesPhysics is not null)
            TotalFramesPhysics.Text = Engine.GetPhysicsFrames().ToString();

        if (TotalFramesProcess is not null)
            TotalFramesProcess.Text = Engine.GetProcessFrames().ToString();

        if (MaxFPS is not null)
            MaxFPS.Text = Engine.MaxFps.ToString();
    }
}
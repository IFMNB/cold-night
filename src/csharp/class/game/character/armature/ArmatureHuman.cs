using Godot;

namespace ColdNight.src.game.character.armature;

[GlobalClass, Icon("res://addons/at-icons/node/person_body.svg")] public partial class ArmatureHuman : ArmatureBuilder
{
    [Export] public StringName RightHand = StringNameExtension.Empty;
    [Export] public StringName LeftHand = StringNameExtension.Empty;
    [Export] public StringName RightFoot = StringNameExtension.Empty;
    [Export] public StringName LeftFoot = StringNameExtension.Empty;
    
    [Export] public StringName Pelvis = StringNameExtension.Empty;
    [Export] public StringName Torso = StringNameExtension.Empty;
    [Export] public StringName Head = StringNameExtension.Empty;
}
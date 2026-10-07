using ColdNight.src.common;
using ColdNight.src.game.character.inventory;
using Godot;

[GlobalClass, Icon("res://addons/at-icons/node/box_wireframe.svg")] public partial class NullItem : Item, ISingletone<NullItem>
{
    public static NullItem Instance {get; private set;} = new();
    public override string Name { get => "NullItem"; }
    public override string Description { get => "Null reference to item information"; }
    public override uint Size => 12;
}
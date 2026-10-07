using System.Collections.Generic;
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.character.inventory;

[GlobalClass, Icon("res://addons/at-icons/node/info.svg")] public abstract partial class Item : Resource
{
    [Export] public virtual string Name {get; private set;} = string.Empty;
    [Export] public virtual string Description {get; private set;} = string.Empty;
    [Export] public virtual PackedScene ModelScene { get; private set; } = null!;
    [Export] public Godot.Collections.Array<string> Tags {get; private set;} = [];
    [Export] public virtual uint Size {get; private set;}

    public ItemStack GetStack (uint Count = 1)
    {
        ItemStack NewStack = new() {Item = this};
        NewStack.Put(Count, out var _);

        return NewStack;
    }
}
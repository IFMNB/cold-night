using ColdNight.src.game.character.inventory;
using Godot;
using System;

namespace ColdNight.src;

[GlobalClass, Icon("res://addons/at-icons/node/person_body.svg")] public partial class Character : Node
{
    [Export] public Inventory Inventory {
        get
        {
            RealInventory ??= [];
            return RealInventory;
        }
        set => RealInventory = value;
    }

    protected virtual Inventory? RealInventory {get;set;}
}

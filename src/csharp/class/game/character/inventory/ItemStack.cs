using Godot;

namespace ColdNight.src.game.character.inventory;

[GlobalClass, Icon("res://addons/at-icons/node/layers.svg")] public partial class ItemStack : Resource
{
    [Export] public Item Item {get; set;} = NullItem.Instance;
    [Export] public uint Count {get; protected set;} = 0;

    public bool Pick (uint Count, out ItemStack PickedStack)
    {
        PickedStack = null!;
        if (Count == 0 || Count > this.Count) return false;

        PickedStack = new ItemStack { Item = Item, Count = Count };
        this.Count -= Count;
        return true;
    }

    public bool Put (uint Count, out uint Remains)
    {
        uint Free = Item.Size > this.Count ? Item.Size - this.Count : 0;
        uint ToPut = System.Math.Min(Count, Free);
        this.Count += ToPut;
        Remains = Count - ToPut;
        return ToPut > 0;
    }
}
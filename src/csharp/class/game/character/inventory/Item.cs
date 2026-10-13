using System.Collections.Generic;
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.character.inventory;

[GlobalClass, Icon("res://addons/at-icons/node/info.svg")] public abstract partial class Item : Resource
{
    /// <summary>
    /// Специальный класс, предполагающий что предмета не существует. Его нельзя создать самостоятельно.
    /// 
    /// <para>
    /// Для создания информации под другие предметы используется конструирование через редактор Godot.
    /// </para>
    /// </summary>
    public partial class NullRef : Item, ISingletone<NullRef>
    {
        public static NullRef Instance {get; private set;} = new();
    }


    [Export] public string Name = string.Empty;
    [Export] public string Description = string.Empty;
    [Export] public PackedScene? ModelScene = null;
    [Export] public Godot.Collections.Array<string> Tags {get; private set;} = [];
    [Export] public uint StackSize = 1;
    [Export] public bool BothHands = false;

    public ItemStack GetStack (uint Count = 1)
    {
        ItemStack NewStack = new() {Item = this};
        NewStack.Put(Count, out var _);

        return NewStack;
    }

    [Export(PropertyHint.Range,"0, 999, 1")] public virtual float PickTime {get; set;}
}
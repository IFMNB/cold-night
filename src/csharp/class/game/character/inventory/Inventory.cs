using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ColdNight.src.game.character.inventory;

[GlobalClass, Icon("res://addons/at-icons/node/backpack.svg")] public partial class Inventory : Resource, IEnumerable<ItemStack>
{
    [Export] public uint SlotCount { get; private set; } = 20;

    [Export] public Godot.Collections.Array<ItemStack> Items {get; private set;} = [];

    public int Occupied => Items.Count;
    public bool IsFull => Items.Count >= SlotCount;

    public Godot.Collections.Array<ItemStack> GetItemStacks () => [.. Items];

    /// <summary>
    /// Сколько всего штук предмета лежит в инвентаре.
    /// </summary>
    public uint CountOf (Item Item)
    {
        uint Total = 0;
        foreach (ItemStack Stack in Items)
            if (Stack.Item == Item) Total += Stack.Count;
        return Total;
    }

    public bool Contains (Item Item, uint Count = 1) => Count > 0 && CountOf(Item) >= Count;

    /// <summary>
    /// Кладёт предмет: сначала докладывает в неполные стаки, потом создаёт новые.
    /// Remains — сколько не влезло. Возвращает true, если добавилось хоть что-то.
    /// </summary>
    public bool Add (Item Item, uint Count, out uint Remains)
    {
        Remains = Count;
        if (Item == null || Item is NullItem || Count == 0) return false;

        foreach (ItemStack Stack in Items)
        {
            if (Remains == 0) break;
            if (Stack.Item != Item) continue;
            Stack.Put(Remains, out Remains);
        }

        while (Remains > 0 && Items.Count < SlotCount)
        {
            ItemStack NewStack = new() { Item = Item };
            if (!NewStack.Put(Remains, out Remains)) break;
            Items.Add(NewStack);
        }

        bool Changed = Remains < Count;
        if (Changed) EmitChanged();
        return Changed;
    }

    public bool Add (ItemStack Stack, out uint Remains) => Add(Stack.Item, Stack.Count, out Remains);

    /// <summary>
    /// Кладёт предмет в слот Index. Если там стак того же предмета, докладывает в него.
    /// Иначе вставляет новый стак перед Index, сдвигая остальные (нужен свободный слот).
    /// Remains: сколько не влезло. Возвращает true, если добавилось хоть что-то.
    /// </summary>
    public bool Insert (int Index, Item Item, uint Count, out uint Remains)
    {
        Remains = Count;
        if (Item == null || Item is NullItem || Count == 0) return false;
        if (Index < 0 || Index > Items.Count) return false;

        if (Index < Items.Count && Items[Index].Item == Item)
        {
            Items[Index].Put(Count, out Remains);
        }
        else if (Items.Count < SlotCount)
        {
            ItemStack NewStack = new() { Item = Item };
            if (!NewStack.Put(Count, out Remains)) return false;
            Items.Insert(Index, NewStack);
        }
        else return false;

        bool Changed = Remains < Count;
        if (Changed) EmitChanged();
        return Changed;
    }

    public bool Insert (int Index, ItemStack Stack, out uint Remains)
        => Insert(Index, Stack.Item, Stack.Count, out Remains);

    /// <summary>
    /// Забирает Count штук из слота Index. Если стак опустел, он удаляется из списка.
    /// </summary>
    public bool Remove (int Index, uint Count, out ItemStack Removed)
    {
        Removed = null!;
        if (Index < 0 || Index >= Items.Count) return false;

        ItemStack Stack = Items[Index];
        if (!Stack.Pick(Count, out Removed)) { Removed = null!; return false; }

        if (Stack.Count == 0) Items.RemoveAt(Index);
        EmitChanged();
        return true;
    }

    /// <summary>Забирает весь стак из слота Index.</summary>
    public bool RemoveAt (int Index, out ItemStack Removed)
    {
        Removed = null!;
        if (Index < 0 || Index >= Items.Count) return false;

        Removed = Items[Index];
        Items.RemoveAt(Index);
        EmitChanged();
        return true;
    }

    /// <summary>
    /// Удаляет ровно Count штук (всё или ничего). Пустые стаки убираются.
    /// </summary>
    public bool Remove (Item Item, uint Count = 1)
    {
        if (!Contains(Item, Count)) return false;

        uint Left = Count;
        for (int i = Items.Count - 1; i >= 0 && Left > 0; i--)
        {
            ItemStack Stack = Items[i];
            if (Stack.Item != Item) continue;

            uint Take = System.Math.Min(Stack.Count, Left);
            if (!Stack.Pick(Take, out _)) continue;

            Left -= Take;
            if (Stack.Count == 0) Items.RemoveAt(i);
        }

        EmitChanged();
        return true;
    }

    public void Clear ()
    {
        Items.Clear();
        EmitChanged();
    }

    public IEnumerator<ItemStack> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
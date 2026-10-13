using ColdNight.src.game.character.inventory;
using Godot;
using System;

namespace ColdNight.src;

/// <summary>
/// Универсальный пакет данных для любого персонажа.
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/person_body.svg")] public abstract partial class UniCharacter : Node
{
    /// <summary>
    /// Инвентарь персонажа. Он всегда привязан к этому конкретному персонажу.
    /// </summary>
    [Export] public Inventory? Inventory {get => current_inventory;
    set
        {
            if (current_inventory == value)
                return;

            current_inventory = value;
            
            if (current_inventory == null)
                current_inventory = new Inventory();

            InventoryChanged?.Invoke();
            EmitSignalScriptInventoryChanged();
        }
    }

    
    /// <summary>
    /// Текущий Stack предметов, который содержится в дополнительной руке у персонажа.
    /// </summary>
    [Export] public ItemStack? CurrentStackLeft {
    get
        {
            if (current_stack_right is not null)
                if (current_stack_right.Item.BothHands)
                    return current_stack_right;

            return current_stack_left;
        }
    set 
        {
            if (current_stack_left == value)
                return;

            current_stack_left = value;

            StackLeftChanged?.Invoke();
            EmitSignalScriptStackLeftChanged();


            if (current_stack_right is not null)
                if (current_stack_right.Item.BothHands)
                    CurrentStackRight = null;

            if (value is not null)
                if (value.Item.BothHands)
                    CurrentStackRight = null;
        }
    }
    /// <summary>
    /// Текущий Stack предметов, который содержится в основной руке у персонажа.
    /// </summary>
    [Export] public ItemStack? CurrentStackRight {
    get
        {
            if (current_stack_left is not null)
                if (current_stack_left.Item.BothHands)
                    return current_stack_left;

            return current_stack_right;
        }
    set
        {
            if (current_stack_right == value)
                return;

            current_stack_right = value;
            StackRightChanged?.Invoke();
            EmitSignalScriptStackRightChanged();

            if (current_stack_left is not null)
                if (current_stack_left.Item.BothHands)
                    CurrentStackLeft = null;

            if (value is not null)
                if (value.Item.BothHands)
                    CurrentStackLeft = null;
        }
    }




    [Signal] public delegate void ScriptInventoryChangedEventHandler();
    [Signal] public delegate void ScriptStackLeftChangedEventHandler();
    [Signal] public delegate void ScriptStackRightChangedEventHandler();
    public event Action? InventoryChanged;
    public event Action? StackRightChanged;
    public event Action? StackLeftChanged;












    Inventory? current_inventory;
    ItemStack? current_stack_left;
    ItemStack? current_stack_right;
}

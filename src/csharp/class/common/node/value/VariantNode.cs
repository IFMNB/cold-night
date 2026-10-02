using System.Collections.Generic;
using Godot;

namespace ColdNight.src.common.value;

/// <summary>
/// Нода, использующаяся только в качестве хранителя какого-либо значения
/// </summary>
/// <typeparam name="T">Любой Godot совместимый объект</typeparam>
[GlobalClass] public partial class VariantNode : ValueNode<Variant>
{
    /// <summary>
    /// Хранимое объектом значение
    /// </summary>
    [Export] public override Variant Value {get => base.Value;set => base.Value = value;}
    [Signal] public delegate void VariantChangedEventHandler(Variant new_value, Variant old_value);
    [Signal] public delegate void VariantRemovedEventHandler(Variant new_value, Variant old_value);
    [Signal] public delegate void VariantNewEventHandler(Variant new_value, Variant old_value);
    [Signal] public delegate void VariantAvailableEventHandler(Variant new_value, Variant old_value);


    public override void _Ready()
    {
        base._Ready();

        ValueChanged += EmitSignalVariantChanged;
        ValueNew += EmitSignalVariantNew;
        ValueRemoved += EmitSignalVariantRemoved;
        ValueAvailable += EmitSignalVariantAvailable;
    }
}
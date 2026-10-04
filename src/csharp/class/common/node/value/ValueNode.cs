using System;
using System.Collections.Generic;
using Godot;

namespace ColdNight.src.common.value;

/// <summary>
/// Нода, использующаяся только в качестве хранителя какого-либо значения.
/// </summary>
[GlobalClass]
public partial class ValueNode : Node, ISwitchable
{
    [Export] public bool Enabled {get;set;} = true;

    [Export] public bool Releseable {get;set;} = true;

    

    /// <summary>
    /// Хранимое объектом значение.
    /// </summary>
    [Export] public Variant Value
    {
        get => RealValue;
        set
        {
            var old = RealValue;

            if (EqualityComparer<Variant>.Default.Equals(value, old))
                return;

            RealValue = value;
            PreviousValue = old;

            if (!Enabled)
                return;

            EmitSignal(SignalName.ValueChanged, value, old);

            if (VariantExtension.IsNull(old))
                EmitSignal(SignalName.ValueNew, value, old);

            if (VariantExtension.IsNull(value))
                EmitSignal(SignalName.ValueRemoved, value, old);

            if (!VariantExtension.IsNull(value))
                EmitSignal(SignalName.ValueAvailable, value, old);
        }
    }

    [Export] public Variant PreviousValue {get => RealPreviousValue; protected set => RealPreviousValue = value;}

    protected virtual Variant RealValue {get;set;}
    protected virtual Variant RealPreviousValue {get;set;}

    /// <summary>
    /// Операция, при которой значение существует в любом случае.
    /// </summary>
    [Signal] public delegate void ValueAvailableEventHandler(Variant value, Variant old);

    /// <summary>
    /// Любое изменение значения.
    /// </summary>
    [Signal] public delegate void ValueChangedEventHandler(Variant value, Variant old);

    /// <summary>
    /// Значение было null, но стало чем-либо.
    /// </summary>
    [Signal] public delegate void ValueNewEventHandler(Variant value, Variant old);

    /// <summary>
    /// Значение стало null.
    /// </summary>
    [Signal] public delegate void ValueRemovedEventHandler(Variant value, Variant old);

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationPredelete)
            if (!Releseable && GetParent() is not null)
            {
                CancelFree();

#if DEBUG
                GD.PushWarning(
                    $"This node cannot be freed, try freeing the parent node\n{GetPath()}"
                );
#endif
            }
    }
}
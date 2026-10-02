using Godot;

namespace ColdNight.src.common.value;

[GlobalClass] public partial class StringNode : ValueNode<string>
{
    [Export] public override string? Value {get => base.Value;set => base.Value = value;}
    [Signal] public delegate void StringChangedEventHandler (string @new, string @old);
    [Signal] public delegate void StringRemovedEventHandler (string @new, string @old);
    [Signal] public delegate void StringNewEventHandler (string @new, string @old);
    [Signal] public delegate void StringAvailableEventHandler (string @new, string @old);


    public override void _Ready()
    {
        base._Ready();
        ValueChanged += EmitSignalStringChanged;
        ValueNew += EmitSignalStringNew;
        ValueRemoved += EmitSignalStringRemoved;
        ValueAvailable += EmitSignalStringAvailable;
    }
}
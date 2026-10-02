using Godot;

namespace ColdNight.src.common.value;

[GlobalClass] public partial class Vector3Node : ValueNode<Vector3>
{
    [Export] public override Vector3 Value {get => base.Value ;set => base.Value = value;}
    [Signal] public delegate void Vector3ChangedEventHandler (Vector3 @new, Vector3 @old);
    [Signal] public delegate void Vector3RemovedEventHandler (Vector3 @new, Vector3 @old);
    [Signal] public delegate void Vector3NewEventHandler (Vector3 @new, Vector3 @old);
    [Signal] public delegate void Vector3AvailableEventHandler (Vector3 @new, Vector3 @old);


    public override void _Ready()
    {
        base._Ready();
        
        ValueChanged += EmitSignalVector3Changed;
        ValueNew += EmitSignalVector3New;
        ValueRemoved += EmitSignalVector3Removed;
        ValueAvailable += EmitSignalVector3Available;
    }
}
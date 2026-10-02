using Godot;

namespace ColdNight.src.common.value;

[GlobalClass] public partial class NodeValue : ValueNode<Node>
{
    [Export] public override Node? Value {get => base.Value;set => base.Value = value;}

    [Signal] public delegate void WatchedReleasedEventHandler();
    [Signal] public delegate void WatchedReplacedEventHandler();
    [Signal] public delegate void WatchedExitedTreeEventHandler();
    [Signal] public delegate void WatchedEnteredTreeEventHandler();


    [Signal] public delegate void WatchedRemovedEventHandler(Node? @new, Node? @old);
    [Signal] public delegate void WatchedAvailableEventHandler(Node? @new, Node? @old);
    [Signal] public delegate void WatchedChangedEventHandler(Node? @new, Node? @old);
    [Signal] public delegate void WatchedNewEventHandler(Node? @new, Node? @old);


       public override void _Ready()
    {
        base._Ready();

        ValueAvailable += EmitSignalWatchedAvailable;
        ValueChanged += EmitSignalWatchedChanged;
        ValueRemoved += EmitSignalWatchedRemoved;
        ValueNew += EmitSignalWatchedNew;

        ValueChanged += OnWatchedValueChanged;

        if (Value != null)
            WatchNode(Value);
    }


    public override void _ExitTree()
    {
        if (Value != null)
            UnwatchNode(Value);

        base._ExitTree();
    }


    private void OnWatchedValueChanged(Node? @new, Node? @old)
    {
        if (@old != null)
        {
            UnwatchNode(@old);

            if (@new == null)
                EmitSignalWatchedReleased();
        }

        if (@new != null)
        {
            WatchNode(@new);

            if (@old != null)
                EmitSignalWatchedReplaced();
        }
    }


    private void WatchNode(Node node)
    {
        node.TreeEntered += EmitSignalWatchedEnteredTree;
        node.TreeExiting += EmitSignalWatchedExitedTree;
    }


    private void UnwatchNode(Node node)
    {
        node.TreeEntered -= EmitSignalWatchedEnteredTree;
        node.TreeExiting -= EmitSignalWatchedExitedTree;
    }
}
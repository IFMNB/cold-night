using Godot;

namespace ColdNight.src.common.value;

/// <summary>
/// Хранит ссылку на ноду
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/box.svg")] public partial class NodeValue : ValueNode
{
    [Export] public new Node? Value {get => GetNodeOrNull(base.Value.AsNodePath()) ;set => base.Value = value?.GetPath() ?? new NodePath();}

    [Signal] public delegate void NodeValueReleasedEventHandler();
    [Signal] public delegate void NodeValueReplacedEventHandler();
    [Signal] public delegate void NodeValueExitedTreeEventHandler();
    [Signal] public delegate void NodeValueEnteredTreeEventHandler();

       public override void _Ready()
    {
        base._Ready();

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


    private void OnWatchedValueChanged(Variant @new, Variant @old)
    {
        var @node_new = @new.AsGodotObject() as Node;
        var @node_old = @old.AsGodotObject() as Node;


        if (@node_old is not null && IsInstanceValid(node_old))
        {
            UnwatchNode(@node_old);

            if (@node_new is null)
                EmitSignalNodeValueReleased();
        }

        if (@node_new is not null && IsInstanceValid(node_new))
        {
            WatchNode(node_new);

            if (node_old is not null && IsInstanceValid(node_old))
                EmitSignalNodeValueReplaced();
        }
    }


    private void WatchNode(Node node)
    {
        node.TreeEntered += EmitSignalNodeValueEnteredTree;
        node.TreeExiting += EmitSignalNodeValueExitedTree;
    }


    private void UnwatchNode(Node node)
    {
        node.TreeEntered -= EmitSignalNodeValueEnteredTree;
        node.TreeExiting -= EmitSignalNodeValueExitedTree;
    }
}
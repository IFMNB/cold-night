using Godot;
using System.Collections.Generic;
using ColdNight.src;
using Godot.Collections;

namespace ColdNight.src.common;

/// <summary>
/// Класс используемый для хранения чего бы то ни было. По сути обычная нода.
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/folder.svg")]
public partial class Folder : Node
{
    /// <summary>
    /// Количество непосредственных детей.
    /// </summary>
    public int ChildCount { get; private set; }

    /// <summary>
    /// Количество всех потомков.
    /// </summary>
    public int DescendantCount { get; private set; }

    /// <summary>
    /// Группы, к которым принадлежит данный узел.
    /// </summary>
    public Array<Godot.StringName> Groups { get; private set; } = [];

    public override void _EnterTree()
    {
        base._EnterTree();

        RefreshRuntimeInfo(this);

        ChildEnteredTree += OnChildTreeChanged;
        ChildExitingTree += OnChildTreeChanged;
    }

    public override void _ExitTree()
    {
        ChildEnteredTree -= OnChildTreeChanged;
        ChildExitingTree -= OnChildTreeChanged;

        base._ExitTree();
    }

    private void OnChildTreeChanged(Node node) => RefreshRuntimeInfo(this);
    

    private static void RefreshRuntimeInfo(Folder folder)
    {
        folder.ChildCount = folder.GetChildCount();
        folder.DescendantCount = 0;

        foreach (var _ in NodeExtension.GetAllDescendants(folder))
            folder.DescendantCount++;

        folder.Groups = folder.GetGroups();
    }
}
using Godot;

namespace ColdNight.src.common;


/// <summary>
/// Управляет состоянием обработки узлов, объединённых в группы.
/// Позволяет централизованно включать и отключать различные виды обработки
/// (<c>_Process</c>, <c>_PhysicsProcess</c>, input callbacks и т. д.)
/// для всех узлов, входящих в указанную группу.
/// </summary>
[GlobalClass]
public partial class GroupControlNode : Node
{
    /// <summary>
    /// Если включено, изменение состояния группы одновременно изменяет
    /// все поддерживаемые виды обработки узлов и свойство <c>ISwitchable.Enabled</c>.
    /// </summary>
    [Export]
    public bool SwitchAll
    {
        get => switch_all_internal;
        set
        {
            switch_all_internal = value;
            try_set_special_field_for_foreach();
        }
    }

    /// <summary>
    /// Определяет, следует ли изменять состояние обычной обработки узлов
    /// через <c>Node.SetProcess</c>.
    /// </summary>
    [Export]
    public bool SwitchProcess
    {
        get => switch_process_internal;
        set
        {
            switch_process_internal = value;
            try_set_special_field_for_foreach();
        }
    }

    /// <summary>
    /// Определяет, следует ли изменять состояние физической обработки узлов
    /// через <c>Node.SetPhysicsProcess</c>.
    /// </summary>
    [Export]
    public bool SwitchPhysics
    {
        get => switch_physics_internal;
        set
        {
            switch_physics_internal = value;
            try_set_special_field_for_foreach();
        }
    }

    /// <summary>
    /// Определяет, следует ли изменять состояние обработки обычного
    /// пользовательского ввода через <c>Node.SetProcessInput</c>.
    /// </summary>
    [Export]
    public bool SwitchInput
    {
        get => switch_input_internal;
        set
        {
            switch_input_internal = value;
            try_set_special_field_for_foreach();
        }
    }

    /// <summary>
    /// Определяет, следует ли изменять состояние обработки shortcut-ввода
    /// через <c>Node.SetProcessShortcutInput</c>.
    /// </summary>
    [Export]
    public bool SwitchShortcutInput
    {
        get => switch_shortcut_input_internal;
        set
        {
            switch_shortcut_input_internal = value;
            try_set_special_field_for_foreach();
        }
    }

    /// <summary>
    /// Определяет, следует ли изменять состояние обработки необработанного
    /// пользовательского ввода через <c>Node.SetProcessUnhandledInput</c>.
    /// </summary>
    [Export]
    public bool SwitchUnhandledInput
    {
        get => switch_unhandled_input_internal;
        set
        {
            switch_unhandled_input_internal = value;
            try_set_special_field_for_foreach();
        }
    }

    /// <summary>
    /// Определяет, следует ли изменять состояние обработки необработанных
    /// событий клавиатуры через <c>Node.SetProcessUnhandledKeyInput</c>.
    /// </summary>
    [Export]
    public bool SwitchUnhandledKeyInput
    {
        get => switch_unhandled_key_input_internal;
        set
        {
            switch_unhandled_key_input_internal = value;
            try_set_special_field_for_foreach();
        }
    }

    /// <summary>
    /// Определяет, следует ли изменять свойство <c>ISwitchable.Enabled</c>
    /// у узлов группы.
    /// </summary>
    [Export]
    public bool DisableNodeWork
    {
        get => disable_node_work_internal;
        set
        {
            disable_node_work_internal = value;
            try_set_special_field_for_foreach();
        }
    }

    protected virtual bool switch_all_internal {get;set;} = false;
    protected virtual bool switch_process_internal {get;set;} = false;
    protected virtual bool switch_physics_internal {get;set;} = false;
    protected virtual bool switch_input_internal {get;set;} = false;
    protected virtual bool switch_unhandled_key_input_internal {get;set;} = false;
    protected virtual bool switch_unhandled_input_internal {get;set;} = false;
    protected virtual bool disable_node_work_internal {get;set;} = false;
    protected virtual bool switch_shortcut_input_internal {get;set;} = false;

    private void try_set_special_field_for_foreach ()
    {
        if (SwitchAll || SwitchProcess || SwitchPhysics || SwitchInput || SwitchShortcutInput || SwitchUnhandledKeyInput || SwitchUnhandledInput || DisableNodeWork)
            ForForeachSomethingOfSwitchIsTrue = true;
        else
            ForForeachSomethingOfSwitchIsTrue = false;
    }

    protected bool ForForeachSomethingOfSwitchIsTrue = false;

    /// <summary>
    /// Набор управляемых групп и их текущих состояний.
    /// Ключом является имя группы, значением — активна ли группа.
    /// </summary>
    [Export]
    protected Godot.Collections.Dictionary<string, bool> Groups = [];

    /// <summary>
    /// Возникает при отключении группы.
    /// </summary>
    /// <param name="Name">Имя отключённой группы.</param>
    [Signal]
    public delegate void GroupDisabledEventHandler(string Name);

    /// <summary>
    /// Возникает при включении группы.
    /// </summary>
    /// <param name="Name">Имя включённой группы.</param>
    [Signal]
    public delegate void GroupEnabledEventHandler(string Name);

    /// <summary>
    /// Возникает при изменении состояния существующей группы.
    /// </summary>
    /// <param name="Name">Имя изменённой группы.</param>
    [Signal]
    public delegate void GroupChangedEventHandler(string Name);

    /// <summary>
    /// Возникает при изменении состава управляемых групп.
    /// Срабатывает при добавлении или удалении группы.
    /// </summary>
    /// <param name="Name">Имя добавленной или удалённой группы.</param>
    [Signal]
    public delegate void ControlChangedEventHandler(string Name);

    /// <summary>
    /// Возникает после добавления новой группы.
    /// </summary>
    /// <param name="Name">Имя добавленной группы.</param>
    [Signal]
    public delegate void GroupAddedEventHandler(string Name);

    /// <summary>
    /// Возникает после удаления группы.
    /// </summary>
    /// <param name="Name">Имя удалённой группы.</param>
    [Signal] public delegate void GroupRemovedEventHandler(string Name);

    private SceneTree? CurrentTree;

    public override void _Ready()
    {
        base._Ready();

        TreeEntered += SetCurrentTreeLambda;
        TreeExited += SetCurrentTreeLambda;

        GroupDisabled += OnGroupDisabled;
        GroupEnabled += OnGroupEnabled;
        GroupAdded += OnGroupAdded;
    }

    private void SetCurrentTreeLambda () => CurrentTree = GetTree();

    protected void OnGroupDisabled (string Name) => ForeachNodesOfGroupSet(Name, false);

    protected void OnGroupEnabled (string Name) => ForeachNodesOfGroupSet(Name, true);

    protected void OnGroupAdded (string Name) => ForeachNodesOfGroupSet(Name, IsGroupActive(Name));

    protected void ForeachNodesOfGroupSet(string Name, bool WorkModeTo = false)
    {
        if (ForForeachSomethingOfSwitchIsTrue)
            if (IsInstanceValid(CurrentTree))
                foreach (var NodeOfGroup in CurrentTree.GetNodesInGroup(Name))
                    NodeSet(NodeOfGroup, WorkModeTo);
    }

    protected void NodeSet (Node NodeOfGroup, bool WorkModeTo = false)
    {
        ISwitchable? CurrentNodeSwitchable = NodeOfGroup as ISwitchable;                    

        if (SwitchAll)
        {
            NodeOfGroup.SetProcess(WorkModeTo);
            NodeOfGroup.SetProcessInput(WorkModeTo);
            NodeOfGroup.SetProcessShortcutInput(WorkModeTo);
            NodeOfGroup.SetProcessUnhandledInput(WorkModeTo);
            NodeOfGroup.SetProcessUnhandledKeyInput(WorkModeTo);
            NodeOfGroup.SetPhysicsProcess(WorkModeTo);
            
            if (CurrentNodeSwitchable is not null)
                CurrentNodeSwitchable.Enabled = WorkModeTo;

            return;
        }

        if (SwitchProcess)
            NodeOfGroup.SetProcess(WorkModeTo);

        if (SwitchInput)
            NodeOfGroup.SetProcessInput(WorkModeTo);

        if (SwitchPhysics)
            NodeOfGroup.SetPhysicsProcess(WorkModeTo);

        if (SwitchShortcutInput)
            NodeOfGroup.SetProcessShortcutInput(WorkModeTo);

        if (SwitchUnhandledInput)
            NodeOfGroup.SetProcessUnhandledInput(WorkModeTo);

        if (SwitchUnhandledKeyInput)
            NodeOfGroup.SetProcessUnhandledKeyInput(WorkModeTo);

        if (DisableNodeWork)
            if (CurrentNodeSwitchable is not null)
                CurrentNodeSwitchable.Enabled = WorkModeTo;
    }

    /// <summary>
    /// Возвращает текущее состояние указанной группы.
    /// </summary>
    /// <param name="Name">Имя группы.</param>
    /// <returns>
    /// <see langword="true"/>, если группа существует и активна;
    /// в противном случае — <see langword="false"/>.
    /// </returns>
    public bool IsGroupActive (string Name)
    {
        if (Groups.TryGetValue(Name, out var result))
            return result;
        else
            return false;
    }

    /// <summary>
    /// Устанавливает состояние группы.
    /// При изменении состояния соответствующие узлы группы
    /// переключаются в указанное рабочее состояние.
    /// </summary>
    /// <param name="Name">Имя группы.</param>
    /// <param name="To">
    /// Новое состояние группы:
    /// <see langword="true"/> — включить;
    /// <see langword="false"/> — отключить.
    /// </param>
    /// <remarks>
    /// Если группа уже находится в указанном состоянии,
    /// операция не выполняется и сигналы не испускаются.
    /// </remarks>
    public void SetGroup (string Name, bool To)
    {
        if (Groups[Name] == To)
            return;

        Groups[Name] = To;

        if (To)
            EmitSignalGroupEnabled(Name);
        else
            EmitSignalGroupDisabled(Name);

        EmitSignalGroupChanged(Name);
    }

    /// <summary>
    /// Удаляет группу из списка управляемых групп.
    /// </summary>
    /// <param name="Name">Имя удаляемой группы.</param>
    /// <remarks>
    /// Если группы с указанным именем не существует, операция не выполняется.
    /// </remarks>
    public void RemoveGroup (string Name)
    {
        if (Groups.TryGetValue(Name, out var result))
        {
            Groups.Remove(Name);
            EmitSignalGroupRemoved(Name);
            EmitSignalControlChanged(Name);
        }
    }

    /// <summary>
    /// Добавляет новую управляемую группу.
    /// </summary>
    /// <param name="Name">Имя добавляемой группы.</param>
    /// <param name="To">
    /// Начальное состояние группы.
    /// По умолчанию группа создаётся выключенной.
    /// </param>
    /// <remarks>
    /// Если группа с указанным именем уже существует,
    /// операция не выполняется.
    /// </remarks>
    public void InsertGroup (string Name, bool To = false) {
        if (Groups.TryGetValue(Name, out var result))
            return;
        else
        {
            Groups.Add(Name, To);
            EmitSignalGroupAdded(Name);
            EmitSignalControlChanged(Name);
        }
        
    }
}
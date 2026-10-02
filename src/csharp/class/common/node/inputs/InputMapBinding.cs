using Godot;
using ColdNight.src.common.value;

namespace ColdNight.src.common.input;

/// <summary>
/// Нода, реализующая подписку на определённый биндинг в Input Map.
///
/// <para>
/// Нода абстрагирует непосредственно получение <see cref="InputEvent"/>
/// от пользователей биндинга и предоставляет сигналы, описывающие
/// начало, окончание и факт получения ввода.
/// </para>
///
/// <para>
/// Значение отслеживаемого действия может быть изменено во время работы
/// ноды. При изменении биндинга испускается <see cref="BindingChangedEventHandler"/>.
/// </para>
[GlobalClass] // поставить Tool когда пофиксят #96306
public partial class InputMapBinding : Node, ISwitchable
{
    /// <summary>
    /// Определяет, должна ли нода обрабатывать входящие события ввода.
    /// </summary>
    [Export] public bool Enabled { get; set; } = true;

    /// <summary>
    /// Последнее входящее событие, соответствующее текущему биндингу.
    ///
    /// <para>
    /// Значение сохраняется независимо от того, было ли событие нажатием
    /// или отпусканием.
    /// </para>
    /// </summary>
    [Export] public InputEvent? Last { get; protected set; }

    /// <summary>
    /// Направление, связанное с данным биндингом.
    ///
    /// <para>
    /// Сам <c>InputMapBinding</c> не использует это значение при обработке
    /// ввода. Оно предназначено для пользователей ноды, которым требуется
    /// связать входное действие с определённым направлением.
    /// </para>
    /// </summary>
    [Export] public Vector3 Direction = Vector3.Zero;

    /// <summary>
    /// Указывает, было ли соответствующее входное событие получено
    /// на текущем шаге обработки.
    ///
    /// <para>
    /// Значение устанавливается в <c>true</c> в течение текущего шага,
    /// если был получен соответствующий <see cref="InputEvent"/>.
    /// После обработки шага значение сбрасывается.
    /// </para>
    /// </summary>
    [Export] public bool EmittedNow { get; set; } = false;

    /// <summary>
    /// Значение в Input Map, которое нода будет отслеживать.
    ///
    /// <para>
    /// Фактическое значение хранится в дочернем <see cref="NodeValue"/>,
    /// а данное свойство предоставляет типизированный доступ к
    /// <see cref="StringNode"/>.
    /// </para>
    ///
    /// <para>
    /// При изменении значения отслеживаемого биндинга автоматически
    /// обновляется подписка на изменение строки.
    /// </para>
    /// </summary>
    [Export]
    public StringNode? Binding
    {
        get => Watch.Value as StringNode;
        set => Watch.Value = value;
    }

    /// <summary>
    /// Хранилище текущего объекта, за которым отслеживается значение биндинга.
    /// </summary>
    private readonly NodeValue Watch = new()
    {
        Name = "BindingNodeValue",
        Releseable = false
    };

    /// <summary>
    /// Сигнал, сообщающий об изменении самого биндинга.
    ///
    /// <para>
    /// Сигнал испускается независимо от того, какое значение находилось
    /// в биндинге ранее. Он обозначает именно факт смены значения биндинга.
    /// </para>
    ///
    /// <param name="new">Новое значение биндинга.</param>
    /// <param name="old">Предыдущее значение биндинга.</param>
    /// </summary>
    [Signal]
    public delegate void BindingChangedEventHandler(string @new, string @old);

    /// <summary>
    /// Сигнал, испускаемый при получении соответствующего события ввода
    /// в состоянии нажатия.
    ///
    /// <param name="event">Полученное событие ввода.</param>
    /// </summary>
    [Signal]
    public delegate void InputBeganEventHandler(InputEvent @event);

    /// <summary>
    /// Сигнал, испускаемый при получении соответствующего события ввода
    /// в состоянии отпускания.
    ///
    /// <param name="event">Полученное событие ввода.</param>
    /// </summary>
    [Signal]
    public delegate void InputEndedEventHandler(InputEvent @event);

    /// <summary>
    /// Сигнал, испускаемый при каждом получении события ввода,
    /// соответствующего текущему биндингу.
    ///
    /// <para>
    /// В отличие от <see cref="InputBeganEventHandler"/> и
    /// <see cref="InputEndedEventHandler"/>, этот сигнал не зависит
    /// от состояния нажатия события.
    /// </para>
    ///
    /// <param name="event">Полученное событие ввода.</param>
    /// </summary>
    [Signal]
    public delegate void InputEmitEventHandler(InputEvent @event);

    /// <summary>
    /// Пытается получить последнее обработанное событие ввода.
    /// </summary>
    ///
    /// <param name="last">
    /// Последнее событие ввода, если оно было получено.
    /// </param>
    ///
    /// <returns>
    /// <c>true</c>, если последнее событие существует;
    /// иначе <c>false</c>.
    /// </returns>
    public bool TryGetLast(out InputEvent? last)
    {
        last = Last;
        return last is not null;
    }

    public override void _Ready()
    {
        base._Ready();

        #if DEBUG
        if (Engine.IsEditorHint())
        {
            StringNode NewBinding = new()
            {
                Name = "ActionStringValue",
                Value = "Unknown"
            };

            AddChild(NewBinding);
            NewBinding.Owner = GetTree().EditedSceneRoot;

            Binding = NewBinding;
            return;
        }
        #endif

        Watch.WatchedAvailable += OnWatchedAvailable;

        AddChild(Watch);
        SetProcessInput(true);

        if (Binding is StringNode initial)
            OnWatchedAvailable(initial, null);
    }

    /// <summary>
    /// Обновляет подписку на изменение текущего объекта биндинга.
    ///
    /// <para>
    /// При замене отслеживаемого <see cref="StringNode"/> старая подписка
    /// удаляется, а для нового объекта создаётся новая.
    /// </para>
    ///
    /// <para>
    /// Если значение нового биндинга отличается от предыдущего,
    /// испускается <see cref="BindingChangedEventHandler"/>.
    /// </para>
    ///
    /// <param name="new">Новый объект биндинга.</param>
    /// <param name="old">Предыдущий объект биндинга.</param>
    /// </summary>
    private void OnWatchedAvailable(Node? @new, Node? @old)
    {
        if (@old is StringNode oldBinding)
            oldBinding.StringChanged -= EmitSignalBindingChanged;

        if (@new is StringNode newBinding)
        {
            newBinding.StringChanged += EmitSignalBindingChanged;

            if (newBinding.Value != prev_input)
            {
                var prev = prev_input;
                prev_input = newBinding.Value;

                EmitSignalBindingChanged(newBinding.Value, prev);
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        #if DEBUG
        if (Engine.IsEditorHint())
            return;
        #endif

        if (!Enabled)
            return;

        var action = Binding?.Value;

        if (string.IsNullOrEmpty(action) || !InputMap.HasAction(action))
            return;

        if (!InputMap.EventIsAction(@event, action))
            return;

        Last = @event;

        EmitSignalInputEmit(@event);

        if (@event.IsPressed())
        {
            EmitSignalInputBegan(@event);
            capture = true;
        }
        else if (@event.IsReleased())
        {
            EmitSignalInputEnded(@event);
            capture = false;
        }

        on_this_step = true;
    }

    public override void _Process(double delta)
    {
        #if DEBUG
        if (Engine.IsEditorHint())
            return;
        #endif

        base._Process(delta);

        if (on_this_step)
            if (!capture)
            {
                on_this_step = false;
                EmittedNow = true;
            }
            else
                EmittedNow = true;
        else
            EmittedNow = false;
    }

    /// <summary>
    /// Указывает, было ли событие получено в текущем шаге.
    /// </summary>
    private bool on_this_step = false;

    /// <summary>
    /// Указывает, находится ли биндинг в состоянии удержания.
    /// </summary>
    private bool capture = false;

    /// <summary>
    /// Предыдущее строковое значение биндинга.
    /// </summary>
    private string? prev_input = string.Empty;
}
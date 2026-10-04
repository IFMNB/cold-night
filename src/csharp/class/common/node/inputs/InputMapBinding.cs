using Godot;

namespace ColdNight.src.common.input;

/// <summary>
/// Нода, реализующая подписку на один или несколько биндингов в Input Map.
/// </summary>
[GlobalClass]
public partial class InputMapBinding : Node, ISwitchable
{
    /// <summary>
    /// Определяет, должна ли нода обрабатывать входящие события ввода.
    /// </summary>
    [Export]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Последнее входящее событие, соответствующее одному из биндингов.
    /// </summary>
    [Export]
    public InputEvent? Last { get; protected set; }

    /// <summary>
    /// Направление, связанное с данным биндингом.
    /// </summary>
    [Export]
    public Vector3 Direction = Vector3.Zero;

    /// <summary>
    /// Указывает, было ли соответствующее входное событие получено
    /// на текущем шаге обработки.
    /// </summary>
    [Export]
    public bool EmittedNow { get; protected set; } = false;

    /// <summary>
    /// Имена действий Input Map, которые отслеживает нода.
    /// Событие считается соответствующим биндингу, если оно
    /// соответствует хотя бы одному действию.
    /// </summary>
    [Export]
    public Godot.Collections.Array<string> Actions { get; set; } = new();

    /// <summary>
    /// Сигнал, испускаемый при получении события ввода
    /// в состоянии нажатия.
    /// </summary>
    [Signal]
    public delegate void InputBeganEventHandler(InputEvent @event);

    /// <summary>
    /// Сигнал, испускаемый при получении события ввода
    /// в состоянии отпускания.
    /// </summary>
    [Signal]
    public delegate void InputEndedEventHandler(InputEvent @event);

    /// <summary>
    /// Сигнал, испускаемый при каждом получении события ввода,
    /// соответствующего одному из действий.
    /// </summary>
    [Signal]
    public delegate void InputEmitEventHandler(InputEvent @event);

    /// <summary>
    /// Пытается получить последнее обработанное событие ввода.
    /// </summary>
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
            return;
        #endif

        SetProcessInput(true);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        #if DEBUG
        if (Engine.IsEditorHint())
            return;
        #endif

        if (!Enabled)
            return;

        foreach (var action in Actions)
        {
            if (string.IsNullOrEmpty(action))
                continue;

            if (!InputMap.HasAction(action))
                continue;

            if (!InputMap.EventIsAction(@event, action))
                continue;

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
            return;
        }
    }

    public override void _Process(double delta)
    {
        #if DEBUG
        if (Engine.IsEditorHint())
            return;
        #endif

        base._Process(delta);

        EmittedNow = on_this_step;

        if (!capture)
            on_this_step = false;
    }

    /// <summary>
    /// Указывает, было ли событие получено в текущем шаге.
    /// </summary>
    private bool on_this_step = false;

    /// <summary>
    /// Указывает, находится ли биндинг в состоянии удержания.
    /// </summary>
    private bool capture = false;
}
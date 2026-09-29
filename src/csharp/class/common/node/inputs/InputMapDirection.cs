using Godot;
using Godot.Collections;

namespace ColdNight.src.common;

/// <summary>
/// Нода, объединяющая направления от нескольких <see cref="InputMapBinding"/>
/// в единое результирующее направление.
///
/// <para>
/// Каждый биндинг предоставляет своё направление через
/// <see cref="InputMapBinding.Direction"/>. На каждом шаге обработки нода
/// складывает направления всех биндингов, у которых <see cref="InputMapBinding.EmittedNow"/>
/// имеет значение <c>true</c>.
/// </para>
///
/// <para>
/// Полученное направление становится доступно через <see cref="Directions"/>
/// и передаётся в <see cref="Output"/>.
/// </para>
/// </summary>
[GlobalClass, Tool]
public partial class InputMapDirection : Node, ISwitchable, IWireSource
{
    /// <summary>
    /// Определяет, включена ли обработка ноды.
    /// </summary>
    [Export]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Текущее результирующее направление всех активных биндингов.
    ///
    /// <para>
    /// При изменении значения предыдущее направление сохраняется в
    /// <see cref="PreviousDirections"/>.
    /// </para>
    /// </summary>
    [Export]
    public Vector3 Directions
    {
        get => directions_now;
        private set
        {
            if (directions_now == value)
                return;

            PreviousDirections = directions_now;
            directions_now = value;
        }
    }

    /// <summary>
    /// Выходной канал, через который передаётся текущее значение
    /// <see cref="Directions"/>.
    /// </summary>
    [Export]
    public WireOut? Output { get; set; }

    /// <summary>
    /// Направление, установленное на предыдущем обновлении.
    ///
    /// <para>
    /// Позволяет получить значение <see cref="Directions"/>, существовавшее
    /// до последнего изменения результирующего направления.
    /// </para>
    /// </summary>
    [Export]
    public Vector3 PreviousDirections { get; private set; }

    /// <summary>
    /// Список биндингов Input Map, используемых для формирования
    /// результирующего направления.
    ///
    /// <para>
    /// На каждом обновлении учитываются только биндинги, у которых
    /// <see cref="InputMapBinding.EmittedNow"/> имеет значение <c>true</c>.
    /// </para>
    /// </summary>
    [Export]
    public Array<InputMapBinding>? Bindings { get; set; } = [];

    public override void _Ready()
    {
        base._Ready();

        #if DEBUG
        if (Engine.IsEditorHint())
        {
            WireOut Output = new() { Name = "Output" };

            this.Output = Output;
            this.AddChild(Output);
            Output.Owner = this.GetTree().EditedSceneRoot;
        }
        #endif
    }

    public override void _Process(double delta)
    {
        #if DEBUG
        if (Engine.IsEditorHint())
            return;
        #endif

        base._Process(delta);

        if (!Enabled)
            return;

        PreviousDirections = Directions;
        var buffer = Vector3.Zero;

        if (Bindings is not null)
            foreach (var i in Bindings)
            {
                if (i.EmittedNow)
                    buffer += i.Direction;
            }

        Directions = buffer;

        if (Output is not null)
            Output.Value = Directions;
    }

    /// <summary>
    /// Внутреннее хранилище текущего результирующего направления.
    /// </summary>
    private Vector3 directions_now = Vector3.Zero;
}
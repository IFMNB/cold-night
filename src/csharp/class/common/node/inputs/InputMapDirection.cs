using Godot;
using Godot.Collections;

namespace ColdNight.src.common.input;

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
[GlobalClass, Icon("res://addons/at-icons/node/arrow_axes.svg")] public partial class InputMapDirection : Node, ISwitchable
{
    /// <summary>
    /// Определяет, включена ли обработка ноды.
    /// </summary>
    [Export]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Текущее результирующее направление всех активных биндингов.
    /// </summary>
    [Export]
    public Vector3 Directions
    {
        get => directions_now;
        protected set => directions_now = value;
    }

    /// <summary>
    /// Выходной канал, через который передаётся текущее значение
    /// <see cref="Directions"/>.
    /// </summary>
    [Export]
    public Wire? Output { get; set; }

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

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!Enabled) return;

        var buffer = Vector3.Zero;

        if (Bindings is not null)
            foreach (var i in Bindings)
                if (i.EmittedNow)
                    buffer += i.Direction;
                

        Directions = buffer;
        if (Output?.IsActive() ?? false)
            Output.Emit(Directions);
    }

    /// <summary>
    /// Внутреннее хранилище текущего результирующего направления.
    /// </summary>
    private Vector3 directions_now = Vector3.Zero;
}
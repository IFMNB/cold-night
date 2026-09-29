using ColdNight.src.common;
using Godot;
using Godot.Collections;

/// <summary>
/// Объект, представляющий исходящий канал значения.
///
/// <para>
/// <c>WireOut</c> распространяет своё текущее значение среди подключённых
/// получателей <see cref="WireIn"/>. При изменении <see cref="VariantNode.Value"/>
/// каждый валидный получатель получает новое значение через
/// <see cref="WireIn.TrySetValue"/>.
/// </para>
///
/// <para>
/// Каждый получатель самостоятельно определяет способ обработки входящего
/// значения через свой <see cref="WireIn.Mode"/>.
/// </para>
/// </summary>
[GlobalClass]
public partial class WireOut : VariantNode
{
    /// <summary>
    /// Список входящих каналов, которым передаётся текущее значение.
    ///
    /// <para>
    /// Невалидные экземпляры автоматически удаляются из списка при попытке
    /// передачи значения.
    /// </para>
    /// </summary>
    [Export]
    public Array<WireIn> Receivers { get; set; } = [];

    /// <summary>
    /// Инициализирует распространение значения среди подключённых получателей.
    ///
    /// <para>
    /// При каждом изменении значения вызывается
    /// <see cref="WireIn.TrySetValue"/> для каждого валидного получателя.
    /// Если получатель больше не является валидным Godot-объектом, он удаляется
    /// из списка <see cref="Receivers"/>.
    /// </para>
    /// </summary>
    public override void _Ready()
    {
        base._Ready();

        ValueAvailable += (@new, @old) =>
        {
            for (int i = Receivers.Count - 1; i >= 0; i--)
            {
                var item = Receivers[i];

                if (IsInstanceValid(item))
                    item.TrySetValue(@new);
                else
                    Receivers.RemoveAt(i);
            }
        };
    }
}
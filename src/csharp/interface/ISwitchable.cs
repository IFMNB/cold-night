using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Объекты этого интерфейса могут быть отключены и включены
/// </summary>
public interface ISwitchable
{
    /// <summary>
    /// Должен ли работать или нет?
    /// </summary>
    [Export] public bool Enabled {get; set;}
}
using ColdNight.src.common;
using Godot;

namespace ColdNight.src;

/// <summary>
/// От этого объекта исходит какой-либо источник данных
/// </summary>
public interface IWireSource
{
    /// <summary>
    /// Исходящий источник информации
    /// </summary>
    [Export] public WireOut? Output {get;set;}
}
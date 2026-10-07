using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game;

/// <summary>
/// Абстрактный класс для всех контроллеров
[GlobalClass, Icon("res://addons/at-icons/node/cpu.svg")] public abstract partial class Controller : Node, ISwitchable
{
    /// <summary>
    /// Для данного дерева классов это свойство означает отключение поведения, которое применяют классы
    /// на свои цели.
    /// 
    /// Отключение отдельных параметров объектов, типа отключение обработки в _Process или других
    /// методах движка означает буквальное отключение, даже если функционал там используется для нужд
    /// самого контроллера
    /// </summary>
    [Export] public bool Enabled {get; set;} = true;
}
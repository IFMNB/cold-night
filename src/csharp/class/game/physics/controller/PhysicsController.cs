using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Абстрактный класс для всех физических контроллеров, влияющих на поведение физического тела
/// </summary>
[GlobalClass] public abstract partial class PhysicsController : Node, ISwitchable
{
    /// <summary>
    /// Для данного дерева классов это свойство означает отключение сил, которые применяют классы
    /// на свои цели.
    /// 
    /// Отключение отдельных параметров объектов, типа отключение обработки в _Process или других
    /// методах движка означает буквальное отключение, даже если функционал там используется для нужд
    /// самого контроллера
    /// </summary>
    [Export] public bool Enabled {get; set;} = true;

    /// <summary>
    /// Инвертировать воздействие контроллера?
    /// </summary>
    [Export] public bool Inverse {get;set;} = false;
    /// <summary>
    /// Показывает, работал ли в текущем физическом кадре контроллер или нет
    /// </summary>
    [Export] public bool Active {get;set;} = false;
    
    [Export] public virtual float MaxForce {get;set;} = 10f;
}
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Абстрактный класс для всех физических контроллеров, влияющих на поведение физического тела
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/atom.svg")] public abstract partial class PhysicsController : Controller
{
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
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game;

public interface IPhysicsController <T> : ISwitchable, ITargetable<T> where T : Node?
{
    /// <summary>
    /// Максимальная сила, с которой контроллер взаимодействует с чем бы то не было
    /// </summary>
    [Export] public float MaxForce {get;set;}
}
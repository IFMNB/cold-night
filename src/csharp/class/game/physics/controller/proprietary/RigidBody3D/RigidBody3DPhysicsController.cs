using ColdNight.src.game.physics;
using Godot;

/// <summary>
/// Линейка физических контроллеров, работающая только с RigidBody3D объектами
/// </summary>
[GlobalClass] public abstract partial class RigidBody3DPhysicsController : PhysicsController
{
    /// <summary>
    /// У этого вида контроллеров цель может быть только <see cref="RigidBody3D"/>, потому что они
    /// зависят от формы управления силами, которые дает физический движок.
    /// </summary>
    [Export] public virtual RigidBody3D? Target {get;set;}
}
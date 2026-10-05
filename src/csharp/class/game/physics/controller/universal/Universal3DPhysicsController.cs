using ColdNight.src.game.physics;
using Godot;

/// <summary>
/// Универсальная линейка контроллеров, способная применять свое воздействие на объекты любого рода в 3D пространстве
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/globe.svg")] public abstract partial class Universal3DPhysicsController : PhysicsController
{
    /// <summary>
    /// Некоторые контроллеры реализуют собственное поведение даже там где это не поддерживается.
    /// </summary>
    protected const string VelocityMetadata = "universal_3d_physics_controllers_velocity";

    /// <summary>
    /// У этого вида контроллеров цель может быть универсальная. Они не зависят от каких-либо частей движка.
    /// </summary>
    [Export] public virtual Node3D? Target {get;set;}
}
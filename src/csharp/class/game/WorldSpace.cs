using Godot;

namespace ColdNight.src.game;

[GlobalClass, Icon("res://addons/at-icons/node3d/planet.svg")] public partial class WorldSpace : Node {

    /// <summary>
    /// У всех объектов имеющих гравитационные 
    /// </summary>
    [Export] public bool PreferToUseOwnGravity = false;

    /// <summary>
    /// Гравитация
    /// </summary>
    [Export] public Vector3 Gravity = Vector3.Down * (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    
    /// <summary>
    /// Направление ветра в общем плане.
    /// </summary>
    [Export] public Vector3 AirDirection {get => RealAirDirection;set => RealAirDirection = value;}
    protected virtual Vector3 RealAirDirection {get;set;} = Vector3.Zero;

}
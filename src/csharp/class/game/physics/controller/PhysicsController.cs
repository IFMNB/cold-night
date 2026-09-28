using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Абстрактный класс для всех физических контроллеров, влияющих на поведение физического тела
/// </summary>
[GlobalClass] public abstract partial class PhysicsController : Node, IPhysicsController<RigidBody3D?>
{
    [Export] public bool Enabled {get; set;} = true;
    [Export] public RigidBody3D? Target {get;set;}
    [Export] public float MaxForce {get;set;} = 10f;

    /// <summary>
    /// Вернет `boolean` результат попытки взять `Target` и саму цель, либо `null`
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryGetTarget(out RigidBody3D? target)
    {
        target = this.Target;
        return IsInstanceValid(target);
    }

    /// <summary>
    /// Попытается установить `Target`, вернет результат попытки установки
    /// </summary>
    /// <param name="target"></param>
    public bool TrySetTarget (RigidBody3D? target)
    {
        if (this.Target != target || this.Target is null)
            {
                this.Target = target;
                return true;
            }
        return false;
    }

}
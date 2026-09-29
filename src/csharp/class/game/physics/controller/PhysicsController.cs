using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Абстрактный класс для всех физических контроллеров, влияющих на поведение физического тела
/// </summary>
[GlobalClass] public abstract partial class PhysicsController : Node, IPhysicsController<RigidBody3D?>
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
    [Export] public RigidBody3D? Target {get;set;}
    [Export] public virtual float MaxForce {get;set;} = 10f;

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
        if (this.Target != target)
            {
                this.Target = target;
                return true;
            }
        return false;
    }

}
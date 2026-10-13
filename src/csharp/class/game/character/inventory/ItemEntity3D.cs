using Godot;

namespace ColdNight.src.game.character.inventory;

/// <summary>
/// Нода оповещает о том, что ее предок является предметом. Что считать предметом и от чего зависит
/// от того, кто будет эту ноду читать.
/// </summary>
/// <param name="EntityItem">Любой предмет.</param>
[GlobalClass, Icon("res://addons/at-icons/node/box_wireframe.svg")] public partial class ItemEntity3D (Item EntityItem) : RigidBody3D {
    /// <summary>
    /// Собственно, информация о предмете.
    /// </summary>
    [Export] public Item Item {get; private set;} = EntityItem;
    /// <summary>
    /// Родитель вместе с этой нодой будут удалены, если истечет таймер. Если выключить, то таймер сбросится.
    /// </summary>
    [Export] public bool AutoRemove {get; set;} = false;
    /// <summary>
    /// Таймер, по истечению которого нода самоуничтожается вместе с родителем. 
    /// </summary>
    [Export] public float RemoveAfter {get; set;} = 100.0f;
    protected float? TimeBeforeRemove = null;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!AutoRemove)
            TimeBeforeRemove = null;
        else
            if (TimeBeforeRemove == null)
                TimeBeforeRemove = RemoveAfter;
            else
                TimeBeforeRemove -= (float)delta;

        if (TimeBeforeRemove <= 0.0f)
        {
            this.QueueFree();
            this.ProcessMode = ProcessModeEnum.Disabled;
        }
    }
}
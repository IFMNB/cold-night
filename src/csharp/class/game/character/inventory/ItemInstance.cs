using Godot;

namespace ColdNight.src.game.character.inventory;

[GlobalClass] public abstract partial class ItemInstance : Resource
{


    public void LeftAction (Node3D World)
    {
        
    }

    public void RightAction (Node3D World)
    {
        
    }

    /// <summary>
    /// В этом цикле обрабатывается постоянная логика. Она будет активироваться каждый раз, когда 
    /// </summary>
    /// <param name="World"></param>
    protected virtual void OnLeftActionCycle (Node3D World) {}
    protected virtual void OnRightActionCycle (Node3D World) {}

    protected virtual void OnLeftAction (Node3D World) {}
    protected virtual void OnRightAction (Node3D World) {}
    
}
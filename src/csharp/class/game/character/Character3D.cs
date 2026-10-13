using Godot;
namespace ColdNight.src.game.character;

/// <summary>
/// Объект представляющий рядового персонажа в 3D мире. Компонует в себе все что необходимо для функционирования персонажа:
/// управление, вооружение, тп
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/person_body.svg"), Tool] public partial class Character3D : UniCharacter {
    public override void _Ready()
    {
        base._Ready();

        #if DEBUG
        
        this.ChildEnteredTree += EditorHint_BootloadBodyChild;
        this.ChildEnteredTree += _ => UpdateConfigurationWarnings();
        this.ChildExitingTree += _ => UpdateConfigurationWarnings();
        this.TreeEntered += UpdateConfigurationWarnings;
        UpdateConfigurationWarnings();

        #endif
    }


    #if DEBUG

    void EditorHint_BootloadBodyChild (Node Child)
    {
        if (this.Body3D is null)
            if (Child is CharacterBody3D then)
                if (then.IsActive())
                    this.Body3D = then;
    }

    public override string[] _GetConfigurationWarnings ()
    {
        if (this.Body3D is null)
            return ["This node does not have related target. Child of Character3D should be PhysicsBody3D node for automatic definition"];
                
        return [];

    }
    #endif

    private PhysicsBody3D? Body3D;
}
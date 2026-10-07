using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.singletone;

/// <summary>
/// Синглтон объект, использующийся в качестве полного определения игрового пространства.
/// 
/// Проще говоря, данные о мире вокруг.
/// </summary>
public partial class Game : Node, ISingletone<Game>
{
    /// <summary>
    /// Синглтон-экземпляр.
    /// 
    /// <para>
    /// Если нет Autoload или он отключен, то обращение к этому синглтону будет выкидывать ошибку.
    /// </para>
    /// </summary>
    public static Game Instance {get; private set;} = null!;

    protected bool InstanceExists = false;

    /// <summary>
    /// Экземпляр объекта мира, повествующий о его поведении. Используется контроллерами.
    /// </summary>
    public WorldSpace WorldSpace {
        get {
            if (!IsInstanceValid(WorldSpaceInstance))
                WorldSpaceInstance = new();

            var parent = WorldSpaceInstance.GetParent();

            if (parent is null)
                this.AddChild(WorldSpaceInstance);
            else if (parent != this)
                WorldSpaceInstance.Reparent(this);

            return WorldSpaceInstance;
        }
    }

    private WorldSpace? WorldSpaceInstance = null;

    public override void _Ready()
    {
        base._Ready();
        
        if (!(Instance?.IsActive() ?? false) && !InstanceExists)
        {
            InstanceExists = true;
            Instance = this;
        }
    }
}
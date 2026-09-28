using Godot;

namespace ColdNight.src.common;

/// <summary>
/// У этого объекта есть `Singletone` объект, который реализуется на уровне класса 1 раз
/// за всю программу
/// </summary>
/// 
/// <typeparam name="T">
/// Любой объект, главное чтобы была реализация синглтона на уровне класса
/// </typeparam>
public interface ISingletone <T> where T : GodotObject?
{
    /// <summary>
    /// Экземпляр объекта на всю программу. Предпочтите его использование, если вам не
    /// важны индивидуальные настройки класса и его объектов, которые вы используете
    /// </summary>
    public static abstract T Instance {get;}
}
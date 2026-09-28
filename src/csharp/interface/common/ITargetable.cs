using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Объект имеет поведение, которое зависит от текущей цели
/// </summary>
/// <typeparam name="T">Любой объект, попадающий под этот тип</typeparam>
public interface ITargetable <T> where T : Node?
{
    /// <summary>
    /// Цель к которой применяется работа объекта.
    /// 
    /// Помните, цель не обязана существовать в момент работы объекта, поэтому используйте
    /// проверки на существование цели перед выполнением работы
    /// </summary>
    [Export] public T Target {get;set;}
}
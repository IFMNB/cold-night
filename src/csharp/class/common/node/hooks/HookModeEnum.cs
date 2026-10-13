namespace ColdNight.src.common.hooks;

/// <summary>
/// Определяет способ работы конкретного хука.
/// </summary>
public enum HookMode 
{
    /// <summary>
    /// Чтение/Запись данных в момент вызова _Process
    /// </summary>
    OnProcess,

    /// <summary>
    /// Чтение/Запись данных в момент вызова _PhysicsProcess
    /// </summary>
    OnPhysicsProcess,

    /// <summary>
    /// Чтение/Запись данных в момент вызова _Ready
    /// </summary>
    OnReady,
    /// <summary>
    /// Чтение/Запись данных в момент вызова _EnterTree
    /// </summary>
    OnEnterTree,
}
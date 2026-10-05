using System;
using System.Collections.Generic;
using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Небольшой класс, реализующий регистр состояний
/// </summary>
[GlobalClass] public partial class StateRegistry : Observer
{
    readonly Dictionary<Enum, bool> states = [];
    public bool GetState (Enum State, out bool result) => states.TryGetValue(State, out result);
    public void SetState (Enum State, bool To)
    {
        if (!states.ContainsKey(State))
        {
            states.Add(State, To);
            Emit(StateRegistryNotification.Added, Convert.ToUInt16(State), To);
        }
        else
        {
            if (states[State] == To) 
                return;

            states.Remove(State);
            states.Add(State, To);
            Emit(StateRegistryNotification.Changed, Convert.ToUInt16(State), To);
        }
    }
    public void RemState (Enum State)
    {
        if (states.ContainsKey(State))
        {
            states.Remove(State);
            Emit(StateRegistryNotification.Removed, Convert.ToUInt16(State));
        }
    }
}

/// <summary>
/// Уведомления об изменении в <see cref="StateRegistry"/>
/// </summary>
public enum StateRegistryNotification
{

    /// <summary>
    /// Когда существующее состояние было изменено
    /// </summary>
    Changed,
    /// <summary>
    /// Когда существующее состояние было удалено
    /// </summary>
    Removed,
    /// <summary>
    /// Когда новое состояние было впервые добавлено
    /// </summary>
    Added
}
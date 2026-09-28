using System;
using System.Collections.Generic;
using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Класс, реализующий простую систему уведомлений для всех его наследников
/// </summary>
[GlobalClass] public partial class Observer : RefCounted
{
    /// <summary>
    /// Отложенные вызовы будут вызывать `Callable` функции методом `<see cref="Callable"/>.CallDeffered()`,
    /// а не обычным `<see cref="Callable"/>.Call()`
    /// </summary>
    [Export] public bool DefferedCalls = true;

    public void Emit(Enum Any, params Variant[] values)
    {
        var zone = GetEnumZone(Any);

        var once = GetModeZone(zone, ObserverConnectionVariant.Once);
        var ever = GetModeZone(zone, ObserverConnectionVariant.Ever);
        var skip = GetModeZone(zone, ObserverConnectionVariant.Skip);
        var next = GetModeZone(zone, ObserverConnectionVariant.Next);

        foreach (var action in once)
            if (DefferedCalls)
                action.CallDeferred(values);
            else
                action.Call(values);

        once.Clear();

        foreach (var action in ever)
            if (DefferedCalls)
                action.CallDeferred(values);
            else
                action.Call(values);

        once.AddRange(skip);
        skip.Clear();

        ever.AddRange(next);
        next.Clear();
    }

    public void Declare (Enum Any, ObserverConnectionVariant Mode, params Callable[] actions) =>
        GetModeZone(GetEnumZone(Any), Mode).AddRange(actions);

    public void Dereference(Enum Any, ObserverConnectionVariant Mode, params Callable[] actions) 
    {
        var zone = GetModeZone(GetEnumZone(Any), Mode);

        foreach (var action in actions)
            zone.Remove(action);
    }



    private readonly Dictionary<Enum, Dictionary<ObserverConnectionVariant, List<Callable>>> handlers = [];
    private Dictionary<ObserverConnectionVariant, List<Callable>> GetEnumZone(Enum key)
    {
        if (!handlers.TryGetValue(key, out var list))
        {
            list = [];
            handlers[key] = list;
        }

        return list;
    }
    private List<Callable> GetModeZone (Dictionary<ObserverConnectionVariant, List<Callable>> Map, ObserverConnectionVariant Mode)
    {
        if (!Map.TryGetValue(Mode, out var list))
        {
            list = [];
            Map[Mode] = list;
        }

        return list;
    }
}

/// <summary>
/// Варианты соединений, которые можно создавать в `<see cref="Observer"/>`
/// </summary>
public enum ObserverConnectionVariant : byte
{

    /// <summary>
    /// Вызвать один раз, после чего удалить
    /// </summary>
    Once = 1,

    /// <summary>
    /// Постоянно вызывать
    /// </summary>
    Ever = 2,

    /// <summary>
    /// Пропустить один раз, в следующий раз вызвать как `Next`
    /// </summary>
    Next = 3,

    /// <summary>
    /// Пропустить один раз, в следующий раз вызвать как `Once`
    /// </summary>
    Skip = 4
}
using System;
using System.Collections.Generic;
using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Нода, использующаяся только в качестве хранителя какого-либо значения
/// </summary>
/// <typeparam name="T">Любой Godot совместимый объект</typeparam>
public abstract partial class ValueNode <T> : Node, ISwitchable
{
    [Export] public bool Enabled {get;set;} = true;

    [Export] public bool Releseable {get;set;} = true;
    
    /// <summary>
    /// Хранимое объектом значение
    /// </summary>
    public virtual T? Value {get => _value; set
        {
            var old = _value;

            if (EqualityComparer<T>.Default.Equals(value, old))
                return;

            _value = value;

            if (!Enabled)
                return;
            
            ValueChanged?.Invoke(value, old);

            if (old is null)
                ValueNew?.Invoke(value, old);

            if (value is null)
                ValueRemoved?.Invoke(value, old);

            if (value is not null)
                ValueAvailable?.Invoke(value, old);
        }
    }

    /// <summary>
    /// Уведомление о операциях, при которых значение существует в любом случае
    /// 
    /// Например, это объединение ValueNew и ValueChanged с условием что Value is not null
    /// </summary>
    public event Action<T?, T?>? ValueAvailable;
    /// <summary>
    /// Уведомление о любых операциях со значением
    /// </summary>
    public event Action<T?, T?>? ValueChanged;
    /// <summary>
    /// Когда Value был null, но затем стал чем-то
    /// </summary>
    public event Action<T?, T?>? ValueNew;
    /// <summary>
    /// Когда Value стал null
    /// </summary>
    public event Action<T?, T?>? ValueRemoved;

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationPredelete)
            if (!Releseable && GetParent() is not null)
            {
                CancelFree();
                
                #if DEBUG 
                GD.PushWarning($"This node cannot be freed, try freeing the parent node \n{GetPath().ToString()}");
                #endif
            }
                CancelFree();        
    }

    private T? _value;
}
using Godot;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ColdNight.src.common.hooks;

/// <summary>
/// Специализированный читатель данных, который читает значения через маппинг <see cref="PropertyName"/> и
/// <see cref="PropertyRead"/>. 
/// 
/// Используется для чтения значений из объектов Godot, которые имеют свойства, соответствующие перечислению <typeparamref name="T"/>.
/// 
/// <para>
/// Для создания релевантных читателей данных просто создайте наследника этого класса с нужным <typeparamref name="T"/> и [GlobalClass] аргументом.
/// Работать все будет автоматически, но для инспектора потребуется добавить
/// 
/// <c>
///     [Export] public new Тип PropertyRead {get => base.PropertyRead;set => base.PropertyRead = value;}
/// </c>
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract partial class GenReader<T> : BaseReader where T : struct, Enum
{
    /// <summary>
    /// Имя свойства, которое будет использоваться для чтения данных из <see cref="Target"/>.
    /// </summary>
    public T PropertyRead { get => _value; set
        {
            _value = value;
            _property = value.ToString() ?? string.Empty;
        } 
    }
    private T _value {get;set;}

    protected override Node? RealTarget { 
        get => _realTarget;
        set {
            _realTarget = value;
            _propertyList = value?.GetPropertyList().ToList();
        } 
    }

    private List<Godot.Collections.Dictionary>? _propertyList = [];
    private Node? _realTarget;
    private string _property = string.Empty;

    public override bool Read(
        Node Target,
        out Variant result)
    {
        StringName propertyName = PropertyRead.ToString();

        if (_propertyList is not null)
            foreach (Godot.Collections.Dictionary property in _propertyList )
            {
                if (property["name"].AsStringName() != propertyName)
                    continue;

                result = Target.Get(propertyName);
                return true;
            }

        result = default;
        return false;
    }
}
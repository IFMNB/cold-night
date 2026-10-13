using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ColdNight.src.common.hooks;

/// <summary>
/// Специализированный писатель данных, который записывает значения через маппинг
/// <see cref="PropertyWrite"/>.
///
/// Используется для записи значений в объекты Godot, которые имеют свойства,
/// соответствующие перечислению <typeparamref name="T"/>.
///
/// <para>
/// Для создания релевантных писателей данных просто создайте наследника этого класса
/// с нужным <typeparamref name="T"/> и [GlobalClass] аргументом.
/// Для отображения свойства в инспекторе потребуется добавить:
///
/// <c>
/// [Export] public new Тип PropertyWrite
/// {
///     get => base.PropertyWrite;
///     set => base.PropertyWrite = value;
/// }
/// </c>
/// 
/// </para>
/// </summary>
/// <typeparam name="T">Перечисление свойств, доступных для записи.</typeparam>
public abstract partial class GenWriter<T> : BaseWriter where T : struct, Enum
{
    /// <summary>
    /// Имя свойства, которое будет использоваться для записи данных в
    /// <see cref="BaseWriter.Target"/>.
    /// </summary>
    public T PropertyWrite
    {
        get => _value;
        set
        {
            _value = value;
            _property = (value.ToString() ?? string.Empty).ToSnakeCase();
        }
    }

    private T _value {get;set;}

    protected override Node? RealTarget
    {
        get => _realTarget;
        set
        {
            _realTarget = value;
        }
    }

    private Node? _realTarget;
    private string _property = string.Empty;

    public override bool Write(
        Node Target,
        Variant value)
    {
        var val = Target.Get(_property);
        Target.Set(_property, value);
        return !VariantExtension.IsNull(val) && !VariantExtension.ValueEqual(val, value);
    }
}
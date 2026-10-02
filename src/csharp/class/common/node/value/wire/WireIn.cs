using Godot;
using ColdNight.src.common.value;

namespace ColdNight.src.common;

/// <summary>
/// Объект, представляющий входящий канал значения.
///
/// <para>
/// <c>WireIn</c> хранит полученное значение и определяет способ его применения
/// к текущему значению через <see cref="Mode"/>.
/// </para>
///
/// <para>
/// При режиме <see cref="OperationMode.Replace"/> входящее значение полностью
/// заменяет текущее. В остальных режимах входящее значение передаётся в
/// <see cref="VariantExtension.TryApply"/>, который выполняет соответствующую
/// операцию над текущим и новым значением.
/// </para>
/// </summary>
[GlobalClass]
public partial class WireIn : VariantNode
{
    /// <summary>
    /// Определяет способ применения входящего значения к текущему значению.
    ///
    /// <para>
    /// При <see cref="OperationMode.Replace"/> текущее значение полностью
    /// заменяется входящим. Остальные режимы выполняют соответствующую
    /// операцию между текущим и входящим значениями.
    /// </para>
    /// </summary>
    [Export] public OperationMode Mode = OperationMode.Replace;

    /// <summary>
    /// Если в пустой <see cref="WireIn"/> передается не <see langword="null"/> значение,
    /// то он принимает его как за эталон и ставит как свой <see cref="Value"/>
    /// </summary>
    [Export] public bool AllocNullAsSample = true;

    /// <summary>
    /// Пытается применить новое значение к текущему значению входа.
    ///
    /// <para>
    /// Если установлен режим <see cref="OperationMode.Replace"/>, значение
    /// заменяется напрямую без дополнительного преобразования.
    /// </para>
    /// 
    /// <para>
    /// Если есть <see cref="AllocNullAsSample"/>, то неважно какой стоит <see cref="Mode"/>,
    /// провод принудительно установит свой <see cref="Value"/> в полученный первый же <paramref name="newValue"/>
    /// если его изначальный <see cref="Value"/> был <see langword="null"/>
    /// </para>
    ///
    /// <para>
    /// Для остальных режимов используется
    /// <see cref="VariantExtension.TryApply"/>. Если операция поддерживается
    /// и выполнена успешно, результат становится новым значением входа.
    /// </para>
    ///
    /// <para>
    /// При невозможности выполнить операцию текущее значение не изменяется,
    /// а метод возвращает <c>false</c>.
    /// </para>
    ///
    /// <param name="newValue">Новое значение, поступившее на вход.</param>
    /// <returns>
    /// <c>true</c>, если значение было успешно применено;
    /// <c>false</c>, если операция не поддерживается или не может быть выполнена.
    /// </returns>
    public bool TrySetValue(Variant newValue)
    {
        if (Mode == OperationMode.Replace)
        {
            Value = newValue;
            return true;
        }

        if (AllocNullAsSample)
            if (Value.VariantType == Variant.Type.Nil)
            {
                Value = newValue;
                return true;
            }

        if (!VariantExtension.TryApply(Value, newValue, Mode, out Variant result))
            return false;

        Value = result;
        return true;
    }
}

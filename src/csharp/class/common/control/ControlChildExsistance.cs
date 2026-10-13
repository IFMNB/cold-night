using System;

namespace ColdNight.src.common.control;

/// <summary>
/// Используется
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="val"></param>
internal class ControlChildExsistance <T> (T val)
{
    /// <summary>
    /// Ожидаемый тип
    /// </summary>
    public readonly T Expected = val;

    public void IsIt (object any)
    {
        Operation?.Invoke();

        if (any is T)
            Success?.Invoke();
        else
            Fail?.Invoke();
    }

    /// <summary>
    /// Любое проишествие
    /// </summary>
    public event Action? Operation;
    /// <summary>
    /// Неуспешное сравнение
    /// </summary>
    public event Action? Fail;
    /// <summary>
    /// Успешное сравнение
    /// </summary>
    public event Action? Success;
}
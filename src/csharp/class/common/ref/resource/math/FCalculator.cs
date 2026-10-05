using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Базовый абстрактный класс для семейства `PID` подобных регуляторов
/// 
/// (Proportional–Integral–Derivative)
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/calculator.svg")] public abstract partial class FCalculator : Resource
{
    /// <summary>
    /// Применяемые к калькулятору коэффициенты. Подробней о них описывает сам калькулятор, который
    /// реализует абстрактный класс `FCalculator`. Это стандартное описание этого поля.
    /// </summary>
    [Export] public Vector3 Coefficients = new(10f, 0f, 5f);

    /// <summary>
    /// Метод конструирует нужную силу, которую необхоидмо применить на цель управляющего доменного кода
    /// </summary>
    /// <param name="target">Что мы хотим?</param>
    /// <param name="current">Что мы имеем?</param>
    /// <param name="delta">Какое время этого шага физики?</param>
    /// <returns>Сила, применяемая для цели доменного кода</returns>
    public abstract float CalculateForce (float target, float current, double delta);
    
    /// <summary>
    /// Этот метод по определению одинаковый для всех контроллеров, потому что он просто применяет
    /// и рассчитывает к полученной силе указанную delta. 
    /// 
    /// Например, в `<see cref="RigidBody3D"/>` метод `ApplyVectorForce()` применяется с учетом <paramref name="delta"/>,
    /// Godot самостоятельно рассчитывает нужное усилие с учетом физического шага. В импульсах же этого не происходит,
    /// поэтому рассчет импульса требует учесть дополнительную переменную.
    /// </summary>
    /// <param name="target">Что мы хотим?</param>
    /// <param name="current">Что мы имеем?</param>
    /// <param name="delta">Какое время этого шага физики?</param>
    /// <returns>Импульс, применяемый для цели доменного кода</returns>
    public float CalculateImpulse(float target, float current, double delta) => CalculateForce(target, current, delta) * (float)delta; 

    /// <summary>Сбрасывает внутреннее состояние (интеграл, прошлую ошибку). Для P ничего не делает.</summary>
    public virtual void Reset() { }
}
using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Калькулятор силы на основе PID-регулятора (Proportional–Integral–Derivative).
/// 
/// Он используется, чтобы автоматически подгонять некоторую величину к заданному значению, где:
///
/// `P` — реагирует на текущую ошибку.
/// `I` — учитывает накопленную ошибку.
/// `D` — реагирует на скорость изменения ошибки.
/// `Kp, Ki, Kd` — коэффициенты настройки.
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/calculator.svg")] public partial class PIDCalculator : FCalculator, ISingletone<PIDCalculator>
{

    public static PIDCalculator Instance { get; } = new();
    private float integral = 0f;
    private float previousError = 0f;
    private bool primed = false;

    public override void Reset() { integral = 0f; previousError = 0f; primed = false; }

    public override float CalculateForce(float target, float current, double delta)
    {
        var fdelta = (float)delta;
        float error = target - current;
        float derivative = primed ? (error - previousError) / fdelta : 0f;

        integral += error * fdelta;
        previousError = error;
        primed = true;

        return Coefficients.X * error + Coefficients.Y * integral + Coefficients.Z * derivative;
    }
}
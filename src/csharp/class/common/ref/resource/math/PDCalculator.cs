using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Калькулятор силы на основе PD-регулятора (Proportional–Derivative).
/// 
/// `PD` - добавляет информацию о том, насколько быстро изменяется ошибка.
/// 
/// Он постоянно сравнивают желаемое значение с текущим и на основе ошибки
/// вычисляют управляющее воздействие, например - силу для `RigidBody3D`
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/calculator.svg")] public partial class PDCalculator : FCalculator, ISingletone<PDCalculator>
{
    public static PDCalculator Instance { get; } = new();
    private float previousError = 0f;
    private bool primed = false;

    public override void Reset() { previousError = 0f; primed = false; }

    public override float CalculateForce(float target, float current, double delta)
    {
        float error = target - current;
        float derivative = primed ? (error - previousError) / (float)delta : 0f;
        previousError = error;
        primed = true;

        return Coefficients.X * error + Coefficients.Y * derivative;
    }
}
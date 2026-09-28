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
[GlobalClass] public partial class PDCalculator : FCalculator, ISingletone<PDCalculator>
{
    public static PDCalculator Instance { get; } = new();
    private float previousError = 0f;

    public override float CalculateForce(float target, float current, double delta)
    {
        float error = target - current;
        float derivative = (error - previousError) / (float)delta;
        previousError = error;

        return
            Coefficients.X * error +
            Coefficients.Y * derivative;
    }
}
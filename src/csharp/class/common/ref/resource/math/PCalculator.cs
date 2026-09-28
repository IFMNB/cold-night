using Godot;

namespace ColdNight.src.common;

/// <summary>
/// Калькулятор силы на основе `P` регулятора (Proportional–Integral–Derivative)
/// </summary>
[GlobalClass] public partial class PCalculator : FCalculator
{
    public override float CalculateForce(float target, float current, double delta) => Coefficients.X * (target - current);
}
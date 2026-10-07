using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Стабилизатор ориентации: удерживает цель в ориентации якоря, гася отклонения PD-регулятором
/// по каждой оси отдельно.
///
/// <para>
/// <see cref="PhysicsController.MaxForce"/> здесь — максимальная скорость коррекции, градусы в секунду
/// (как в <see cref="OrientationController"/>). Значение по умолчанию 10 очень мало, для стабилизации
/// поставьте в инспекторе побольше. За один кадр ось не перескакивает через ноль.
/// </para>
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/orbit.svg")]
public partial class OrientationStabilizerController : OrientationAnchorController
{
    [Export] public PDCalculator PitchCalculator { get; set; } = new();
    [Export] public PDCalculator YawCalculator { get; set; } = new();
    [Export] public PDCalculator RollCalculator { get; set; } = new();

    protected override Quaternion Correct(Quaternion deviation, double delta)
    {
        if (IgnoresAll)
            return deviation;

        Vector3 euler = Decompose(deviation);

        return Compose(new Vector3(
            Step(euler.X, IgnoreX, PitchCalculator, delta),
            Step(euler.Y, IgnoreY, YawCalculator, delta),
            Step(euler.Z, IgnoreZ, RollCalculator, delta)));
    }

    private float Step(float angle, bool ignore, PDCalculator calculator, double delta)
    {
        if (ignore)
            return angle;

        float step = Mathf.Clamp(calculator.CalculateForce(0f, angle, delta), -MaxForce, MaxForce) * (float)delta;
        float limit = Mathf.Abs(angle);

        return angle + Mathf.Clamp(step, -limit, limit);
    }

    protected override void OnStopped()
    {
        PitchCalculator.Reset();
        YawCalculator.Reset();
        RollCalculator.Reset();
    }
}
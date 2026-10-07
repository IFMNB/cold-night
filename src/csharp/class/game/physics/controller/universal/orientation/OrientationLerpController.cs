using Godot;

namespace ColdNight.src.game.physics;

[GlobalClass, Icon("res://addons/at-icons/node3d/rotate.svg")]
public partial class OrientationLerpController : OrientationAnchorController
{
    /// <summary>
    /// Скорость подтягивания, 1/с.
    /// За 1/Speed секунд остаётся около 37% ошибки.
    /// </summary>
    [Export(PropertyHint.Range, "0,60,0.1,or_greater")]
    public float Speed { get; set; } = 10f;

    protected override Quaternion Correct(Quaternion deviation, double delta)
    {
        if (IgnoresAll)
            return deviation;

        float t = 1f - Mathf.Exp(
            -Mathf.Max(Speed, 0f) * (float)delta);

        Basis current = new(deviation);
        Basis lerped = new(
            current.X.Lerp(Vector3.Right, t),
            current.Y.Lerp(Vector3.Up, t),
            current.Z.Lerp(Vector3.Back, t));

        Basis corrected = new(
            IgnoreX ? current.X : lerped.X,
            IgnoreY ? current.Y : lerped.Y,
            IgnoreZ ? current.Z : lerped.Z);

        return corrected
            .Orthonormalized()
            .GetRotationQuaternion();
    }
}
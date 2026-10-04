using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Контроллер который используется для ограничения параметров ориентации любого объекта.
/// 
/// <para>
/// Он работает относительно родителя, то есть локально для своей цели. Это значит, что он поддерживает
/// совершенно любые махинации с ориентацией, и будет ограничивать именно то что подразумевалось при начальной настройке.
/// </para>
/// </summary>
[GlobalClass] public partial class OrientationLimitController : Universal3DPhysicsController
{
    [Export] public bool LimitYaw {get;set;} = true;
    [Export] public bool LimitPitch {get;set;} = true;
    [Export] public bool LimitRoll {get;set;} = true;

    [Export] public PDCalculator YawCalculator { get; set; } = new();
    [Export] public PDCalculator PitchCalculator { get; set; } = new();
    [Export] public PDCalculator RollCalculator { get; set; } = new();

    [Export] public Vector2 YawLimit
    {
        get => RealYawLimit;
        set => RealYawLimit = new Vector2(
            Mathf.Clamp(value.X, -360f, 360f),
            Mathf.Clamp(value.Y, -360f, 360f)
        );
    } 
    [Export] public Vector2 PitchLimit
    {
        get => RealPitchLimit;
        set => RealPitchLimit = new Vector2(
            Mathf.Clamp(value.X, -360f, 360f),
            Mathf.Clamp(value.Y, -360f, 360f)
        );
    }

    [Export] public Vector2 RollLimit
    {
        get => RealRollLimit;
        set => RealRollLimit = new Vector2(
            Mathf.Clamp(value.X, -360f, 360f),
            Mathf.Clamp(value.Y, -360f, 360f)
        );
    }

    [Export] public Vector2 HardLimit
    {
        get => RealHardLimit;
        set => RealHardLimit = new Vector2(
            Mathf.Clamp(value.X, -30f, 30f),
            Mathf.Clamp(value.Y, -30f, 30f)
        );
    }

    /// <summary>
    /// MinMax before clamping to hard limit. The amount of allowed crossing the border.
    /// 
    /// <para>
    /// This is the limit that will be used for calculating the force to apply to the target.
    /// If the target is outside of this limit, it will be clamped to the hard limit.
    /// </para>
    /// 
    /// <para>
    /// Before applying the hard limit, the target's rotation will be forced by the PD calculators to stay within this limit.
    /// </para>
    /// </summary>
    protected virtual Vector2 RealHardLimit {get;set;} = new(-30f, 30f);
    protected virtual Vector2 RealYawLimit {get;set;} = new(-360f, 360f);
    protected virtual Vector2 RealPitchLimit {get;set;} = new(-360f, 360f);
    protected virtual Vector2 RealRollLimit {get;set;} = new(-360f, 360f);

public override void _PhysicsProcess(double delta)
{
    base._PhysicsProcess(delta);

    if (!Enabled || !IsInstanceValid(Target))
        {
            Active = false;
            return;
        }

    Active = true;
        

    Vector3 rotation = Target!.RotationDegrees;

    float yawForce = LimitYaw
        ? CalculateLimitForce(rotation.Y, RealYawLimit, YawCalculator, delta)
        : 0f;

    float pitchForce = LimitPitch
        ? CalculateLimitForce(rotation.X, RealPitchLimit, PitchCalculator, delta)
        : 0f;

    float rollForce = LimitRoll
        ? CalculateLimitForce(rotation.Z, RealRollLimit, RollCalculator, delta)
        : 0f;

    rotation.Y += yawForce * (float)delta;
    rotation.X += pitchForce * (float)delta;
    rotation.Z += rollForce * (float)delta;

    rotation.Y = LimitYaw
        ? ApplyHardLimit(rotation.Y, RealYawLimit, RealHardLimit)
        : rotation.Y;

    rotation.X = LimitPitch
        ? ApplyHardLimit(rotation.X, RealPitchLimit, RealHardLimit)
        : rotation.X;

    rotation.Z = LimitRoll
        ? ApplyHardLimit(rotation.Z, RealRollLimit, RealHardLimit)
        : rotation.Z;

    Target.RotationDegrees = Inverse ? -rotation : rotation;
}

        private static float NormalizeAngle(float angle)
    {
        return Mathf.Wrap(angle, -180f, 180f);
    }

    private static float GetLimitedTarget(float angle, Vector2 limit)
    {
        angle = NormalizeAngle(angle);

        float min = Mathf.Min(limit.X, limit.Y);
        float max = Mathf.Max(limit.X, limit.Y);

        return Mathf.Clamp(angle, min, max);
    }

    private static float GetHardLimitedAngle(
        float angle,
        Vector2 limit,
        Vector2 hardLimit)
    {
        angle = NormalizeAngle(angle);

        float min = Mathf.Min(limit.X, limit.Y);
        float max = Mathf.Max(limit.X, limit.Y);

        float hardMin = min + hardLimit.X;
        float hardMax = max + hardLimit.Y;

        return Mathf.Clamp(angle, hardMin, hardMax);
    }

    private static float CalculateLimitForce(
        float angle,
        Vector2 limit,
        PDCalculator calculator,
        double delta)
    {
        angle = NormalizeAngle(angle);

        float target = GetLimitedTarget(angle, limit);

        return calculator.CalculateForce(
            target,
            angle,
            delta
        );
    }

    private static float ApplyHardLimit(
        float angle,
        Vector2 limit,
        Vector2 hardLimit)
    {
        angle = NormalizeAngle(angle);

        float min = Mathf.Min(limit.X, limit.Y);
        float max = Mathf.Max(limit.X, limit.Y);

        return Mathf.Clamp(
            angle,
            min + hardLimit.X,
            max + hardLimit.Y
        );
    }
}
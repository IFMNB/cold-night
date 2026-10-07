using ColdNight.src.common;
using ColdNight.src.game.singletone;
using Godot;

namespace ColdNight.src.game.character;

/// <summary>
/// Контроллер для передвижения объектов класса <see cref="CharacterBody3D"/>
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/arrow_cross.svg")] public partial class MoveControllerCB3D : Controller, ITargetable<CharacterBody3D?>
{
    [Export] public CharacterBody3D? Target {get;set;}

    [Export] public float MaxSpeed {get;set;} = 0.0f;

    [Export] public float Acceleration { get; set; } = 30.0f;
    [Export] public float Deceleration { get; set; } = 40.0f;

    /// <summary>
    /// Высота прыжка (длина вектора). Начальная скорость считается как sqrt(2 * g * h).
    /// </summary>
    [Export] public Vector3 JumpHeight { get; set; } = new Vector3(0.0f, 8.0f, 0.0f);

    [Export] public bool OverrideGravity = true;
    [Export] public Vector3 Gravity { 
        get {
            if (OverrideGravity)
                return RealGravity;
            else if (Game.Instance.WorldSpace.PreferToUseOwnGravity)
                return RealGravity;
            else
                return Game.Instance.WorldSpace.Gravity;
        } set => RealGravity = value;
    }
    protected virtual Vector3 RealGravity {get;set;} = new Vector3(0.0f, 20.0f, 0.0f);

    [Export] public bool Jump = false;
    public bool JumpedEarly {get; protected set;} = false;

    /// <summary>
    /// Время в котором цель все еще может прыгнуть вверх даже если она не стоит на поверхности.
    /// </summary>
    [Export] public float CoyoteTime = 0.1f;
    protected float CoyoteElapsedTime = 0.0f;
    
    /// <summary>
    /// Провод для передачи направления
    /// </summary>
    [Export] public Wire? DirectionInput {get;set;} = null;

    /// <summary>
    /// Направление куда будет совершать движение объект. Нормализуется.
    /// </summary>
    [Export] public Vector3 Direction {get => RealDirection; set
        {
            RealDirection = value;
            NormalizedDirection = value.IsNormalized() ? value : value.Normalized();
        }
    }

    /// <summary>
    /// Если true, <see cref="Direction"/> задаётся относительно поворота цели
    /// (вперёд = -Z, вправо = +X). Если false, направление мировое.
    /// </summary>
    [Export] public bool DirectionIsLocal {get;set;} = true;

    public Vector3 NormalizedDirection {get; protected set;}

    protected virtual Vector3 RealDirection {get;set;} = Vector3.Zero;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (DirectionInput?.IsActive() ?? false)
            if (VariantExtension.TryApply<Vector3>(Direction, DirectionInput.GetReceivedEverPos(0), DirectionInput.ReceiverMode, out var result))
                Direction = result;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        if (!Enabled || !(Target?.IsActive() ?? false)) return;

        float deltaf = (float)delta;
        Vector3 gravity = Gravity;
        Vector3 up = gravity.Normalized();
        Vector3 velocity = Target.Velocity;

        if (!Target.IsOnFloor())
        {
            if (!JumpedEarly)
                CoyoteElapsedTime += deltaf;

            velocity -= gravity * deltaf;
        }
        else
        {
            CoyoteElapsedTime = 0.0f;
            JumpedEarly = false;

            Vector3 horizontal = new(velocity.X, 0.0f, velocity.Z);
            Vector3 worldDirection = DirectionIsLocal ? Target.GlobalBasis * Direction : Direction;

            worldDirection.Y = 0.0f;
            Vector3 targetVelocity = worldDirection * MaxSpeed;

            float rate = Direction.LengthSquared() > 0.0f ? Acceleration : Deceleration;
            horizontal = horizontal.MoveToward(targetVelocity, rate * deltaf);

            velocity.X = horizontal.X;
            velocity.Z = horizontal.Z;
        }

        if (Jump && CoyoteElapsedTime < CoyoteTime && !JumpedEarly)
        {
            CoyoteElapsedTime = CoyoteTime;
            JumpedEarly = true;
            velocity += up * Mathf.Sqrt(2.0f * gravity.Length() * JumpHeight.Length());
        }

        Jump = false;

        Target.Velocity = velocity;
        Target.MoveAndSlide();
    }
}
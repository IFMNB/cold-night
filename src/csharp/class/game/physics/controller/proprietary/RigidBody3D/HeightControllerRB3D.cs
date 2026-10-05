using System.Diagnostics.CodeAnalysis;
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Контроллер, поддерживающий заданную высоту физического тела над поверхностью.
/// <para>
/// Контроллер определяет расстояние до поверхности с помощью <see cref="RayCast3D"/>
/// и рассчитывает необходимую силу по разнице между текущей и заданной высотой
/// с использованием PID-регулятора.
/// </para>
/// <para>
/// Направление воздействия определяется либо мировой осью Y, либо локальной осью Y
/// целевого тела через <see cref="UseWorldUp"/>.
/// </para>
/// <para>
/// Если поверхность не обнаружена в пределах <see cref="MaxRayLength"/>, контроллер
/// либо не применяет силу, либо, при включённом <see cref="UseStrictHeightControl"/>,
/// прикладывает максимальную силу в направлении уменьшения высоты.
/// </para>
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node3d/arrow_up_from_line.svg")] public partial class HeightControllerRB3D : PhysicsControllerRB3D, ICalculatedPID, IRayCast3DCompatible
{

    /// <summary>
    /// Максимальная дистанция луча. Если ни во что не попали — считаем, что летим высоко
    /// </summary>
    [Export] public float MaxRayLength = 2f;

    /// <summary>
    /// Ожидаемая высота над поверхностью
    /// </summary>
    [Export] public float ExpectedHeight = 1f;

    /// <summary>
    /// Исключает использование `GlobalBasis.Y` у `Target`, вместо этого задавая направление высоты
    /// по мировым координатам
    /// </summary>
    [Export] public bool UseWorldUp = true;

    /// <summary>
    /// Если высота превышает длинну луча, то предполагается что мы летим не бесконечно вниз без
    /// каких-либо препятствий, а просто находимся слишком высоко над полом. В этом случае мы применяем
    /// силу `PID` рассчета чтобы максимально уточнить высоту на заданную относительно пола
    /// </summary>
    [Export] public bool UseStrictHeightControl = false;

    [Export] public RayCast3D? ReferencedRayCastInfo {get;set;}

    [Export] public PIDCalculator Calculator {get; set;} = new();

    /// <summary>
    /// Возвращает силу, с которой на тело нужно влиять через `ApplyCentralForce()`
    /// 
    /// Контроллер сам по себе применяет эту силу в процессе рассчета физики
    /// </summary>
    /// <param name="delta">DeltaTime</param>
    public float GetForce (double delta)
    {
        if (!IsInstanceValid(Target))
            return 0f;

        var space = Target.GetWorld3D()?.DirectSpaceState;
        if (space is null)
            return 0f;

        FastRayCast3D result;

        if (IsInstanceValid(ReferencedRayCastInfo))
            result = FastRayCast3D.From(ReferencedRayCastInfo);
        else
        {
            result = FastRayCast3D.From(
                space,
                Target.GlobalPosition,
                -( UseWorldUp ? Vector3.Up : Target.GlobalBasis.Y.Normalized()) * MaxRayLength,
                [.. NodeExtension.GetCollisionRids(Target)]);
        }

        if (!result.Hit)
            if (UseStrictHeightControl)
                return -MaxForce;
            else 
                return 0f;

        return Mathf.Clamp(Calculator.CalculateForce(ExpectedHeight, result.Distance!.Value, (float)delta), -MaxForce, MaxForce);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        if (Enabled)
            if (Target is RigidBody3D target)
                if (target!.IsInsideTree())
                {
                    Active = true;
                    var force = (UseWorldUp ? Vector3.Up : target.GlobalBasis.Y.Normalized()) * GetForce(delta);
                    target!.ApplyCentralForce(Inverse ? -force : force);
                    return;
                }

        Active = false;
    }
}
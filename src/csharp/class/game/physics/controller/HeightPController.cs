using System.Diagnostics.CodeAnalysis;
using ColdNight.src.common;
using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Контроллер высоты для физических тел
/// </summary>
[GlobalClass] public partial class HeightPController : PhysicsController, ICalculatedPID, IRayCast3DCompatible
{

    /// <summary>
    /// Максимальная дистанция луча. Если ни во что не попали — считаем, что летим высоко
    /// </summary>
    [Export] public float MaxRayLength = 10f;

    /// <summary>
    /// Ожидаемая высота над поверхностью
    /// </summary>
    [Export] public float ExpectedHeight = 10f;

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
            if (TryGetTarget(out var target))
                if (target!.IsInsideTree())
                    target!.ApplyCentralForce((UseWorldUp ? Vector3.Up : target.GlobalBasis.Y.Normalized()) * GetForce(delta));
    }
}
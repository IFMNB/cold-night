using Godot;

namespace ColdNight.src.game.physics;

/// <summary>
/// Контроллер, реализующий демпфирование линейного движения объекта. Частный случай
/// самого <see cref="MovePController"/>. Основан на нем же.
/// 
/// <para>
/// Контроллер автоматически использует текущую линейную скорость цели как направление
/// воздействия и стремится уменьшить её проекцию до нулевой скорости. Управление
/// контроллером предполагается через настройку <see cref="PDCalculator"/>, а именно коэффициентов
/// </para>
/// <para>
/// В отличие от <see cref="MovePController"/>, направление не задаётся напрямую:
/// оно определяется текущим значением <see cref="RigidBody3D.LinearVelocity"/> цели.
/// При скорости ниже <see cref="MinSpeed"/> воздействие не обновляется.
/// </para>
/// </summary>
[GlobalClass] public partial class RigidBody3DDamperController : RigidBody3DMoveController{
    /// <summary>
    /// Минимальная скорость цели, при которой демпфер обновляет направление воздействия.
    /// 
    /// Если длина линейной скорости цели меньше этого значения, текущее направление
    /// демпфирования сохраняется.
    /// </summary>
    [Export] public float MinSpeed { get; set; } = 0.01f;
    protected override float TargetSpeed => 0f;
    
    public override void _PhysicsProcess(double delta)
    {
        if (Target is RigidBody3D t && t!.IsInsideTree() && t.LinearVelocity.LengthSquared() > MinSpeed * MinSpeed)
            Direction = t.LinearVelocity;
        
        base._PhysicsProcess(delta);
    }
}
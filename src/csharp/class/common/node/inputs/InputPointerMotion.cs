using Godot;

namespace ColdNight.src.common.input;

/// <summary>
/// Источник данных о движении указателя пользователя.
/// <para>
/// Обрабатывает события движения мыши или иного устройства и предоставляет величину перемещения
/// и скорость движения в двумерном и трёхмерном представлениях.
/// </para>
/// <para>
/// Трёхмерные значения являются представлением соответствующих двумерных
/// величин в плоскости XY: координата Z всегда равна нулю.
/// </para>
/// </summary>
[GlobalClass] public partial class InputPointerMotion : Node, ISwitchable, IWireSource
{
    /// <summary>
    /// Определяет, обрабатывает ли объект события ввода.
    /// <para>
    /// При отключении входные события игнорируются и состояние объекта
    /// не изменяется.
    /// </para>
    /// </summary>
    [Export] public bool Enabled {get;set;} = true;

    /// <summary>
    /// Выходной канал, через который передаётся текущее перемещение указателя
    /// в трёхмерном представлении.
    /// <para>
    /// Для данного класса выход содержит значение <see cref="Delta3"/>.
    /// </para>
    /// </summary>
    [Export] public WireOut? Output {get;set;}

    /// <summary>
    /// Величина перемещения указателя в двумерном пространстве.
    /// <para>
    /// При изменении значения автоматически обновляются <see cref="Delta3"/>
    /// и <see cref="DeltaDirection"/>, после чего испускается сигнал
    /// <see cref="DeltaChangedEventHandler"/>.
    /// </para>
    /// </summary>
    [Export] public Vector2 Delta2 {get => RealDelta2;protected set
    {
        RealDelta2 = new Vector2(-value.Y, -value.X);
        Delta3 = new Vector3(-value.Y, -value.X, 0f);
        DeltaDirection = Delta3.Normalized();
        EmitSignalDeltaChanged(RealDelta2, Delta3);
    }}

    /// <summary>
    /// Величина перемещения указателя в трёхмерном представлении.
    /// <para>
    /// Значение получается из <see cref="Delta2"/> преобразованием
    /// двумерного вектора в плоскость XY.
    /// </para>
    /// </summary>
    [Export] public Vector3 Delta3 {get; protected set;} = Vector3.Zero;

    /// <summary>
    /// Нормализованное направление перемещения указателя.
    /// <para>
    /// Представляет направление <see cref="Delta3"/> без учёта его величины.
    /// Если перемещение отсутствует, направление равно <see cref="Vector3.Zero"/>.
    /// </para>
    /// </summary>
    [Export] public Vector3 DeltaDirection {get; protected set;} = Vector3.Zero;

    /// <summary>
    /// Скорость движения указателя в двумерном пространстве.
    /// <para>
    /// При изменении значения автоматически обновляются <see cref="Velocity3"/>
    /// и <see cref="VelocityDirection"/>, после чего испускается сигнал
    /// <see cref="VelocityChangedEventHandler"/>.
    /// </para>
    /// </summary>
    [Export] public Vector2 Velocity2 {get => RealVelocity2;protected set
    {
        RealVelocity2 = value;
        Velocity3 = new Vector3(value.X, value.Y, 0f);
        VelocityDirection = Velocity3.Normalized();
        EmitSignalVelocityChanged(RealVelocity2, Velocity3);
    }}

    /// <summary>
    /// Скорость движения указателя в трёхмерном представлении.
    /// <para>
    /// Значение получается из <see cref="Velocity2"/> преобразованием
    /// двумерного вектора в плоскость XY.
    /// </para>
    /// </summary>
    [Export] public Vector3 Velocity3 {get;protected set;} = Vector3.Zero;

    /// <summary>
    /// Нормализованное направление движения указателя.
    /// <para>
    /// Представляет направление <see cref="Velocity3"/> без учёта скорости.
    /// Если скорость отсутствует, направление равно <see cref="Vector3.Zero"/>.
    /// </para>
    /// </summary>
    [Export] public Vector3 VelocityDirection {get; protected set;} = Vector3.Zero;

    /// <summary>
    /// Испускается при изменении величины перемещения указателя.
    /// </summary>
    /// <param name="Delta2">
    /// Перемещение указателя в двумерном пространстве.
    /// </param>
    /// <param name="Delta3">
    /// То же перемещение, представленное как трёхмерный вектор в плоскости XY.
    /// </param>
    [Signal] public delegate void DeltaChangedEventHandler(Vector2 Delta2,Vector3 Delta3);

    /// <summary>
    /// Испускается при изменении скорости движения указателя.
    /// </summary>
    /// <param name="Velocity2">
    /// Скорость указателя в двумерном пространстве.
    /// </param>
    /// <param name="Velocity3">
    /// Та же скорость, представленная как трёхмерный вектор в плоскости XY.
    /// </param>
    [Signal] public delegate void VelocityChangedEventHandler(Vector2 Velocity2,Vector3 Velocity3);

    protected Vector2 RealDelta2 = Vector2.Zero;
    protected Vector2 RealVelocity2 = Vector2.Zero;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Enabled)
            return;

        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            return;

        if (@event is InputEventMouseMotion motion)
        {
            Delta2 = motion.Relative;
            Velocity2 = motion.Velocity;
        } else if (@event is InputEventJoypadMotion joypadMotion)
        {
            GD.PushError("cannot handle joypad right now, unsupported + TODO");
        }

        if (IsInstanceValid(Output))
            Output.Value = Delta3;
    }
}
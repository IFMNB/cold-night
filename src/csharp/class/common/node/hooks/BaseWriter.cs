using Godot;

namespace ColdNight.src.common.hooks;

/// <summary>
/// Базовый класс для всех писателей данных, которые могут получать значения
/// из источников данных и записывать их в целевые объекты.
/// 
/// Наследники этого класса используются в системе "проводов" для получения
/// данных из других объектов и передачи их в другие объекты.
/// </summary>
[GlobalClass]
public abstract partial class BaseWriter : Node, ITargetable<Node?>
{
    [Export] public bool Enabled {get;set;} = true;
    [Export] public HookMode Mode {get;set;} = HookMode.OnProcess;
    [Export] public Wire? Input {get;set;}
    [Export] public Variant LastWrite {get;set;}
    [Export] public Node? Target {get => RealTarget;set => RealTarget = value;}

    /// <summary>
    /// Реальная цель для записи данных. Может быть установлена вручную
    /// или автоматически через <see cref="Target"/>.
    /// </summary>
    protected virtual Node? RealTarget {get;set;}

    /// <summary>
    /// Тип данных, которые ожидаются при записи в <see cref="Target"/>.
    /// Неподходящий тип данных будет проигнорирован и не будет записан
    /// в <see cref="LastWrite"/> и <see cref="Target"/>.
    /// </summary>
    [Export] public Godot.Variant.Type ExpectedType {get;set;} = Godot.Variant.Type.Max;

    /// <summary>
    /// Попытка записать <paramref name="value"/> в <paramref name="Target"/>.
    /// </summary>
    /// <param name="Target">Цель для записи данных.</param>
    /// <param name="value">Значение для записи.</param>
    /// <returns>Успешность операции записи.</returns>
    public abstract bool Write(Node Target, Variant value);

    public override void _Ready() => DoOperation(this, HookMode.OnReady);
    public override void _EnterTree() => DoOperation(this, HookMode.OnEnterTree);
    public override void _PhysicsProcess(double delta) => DoOperation(this, HookMode.OnPhysicsProcess);
    public override void _Process(double delta) => DoOperation(this, HookMode.OnProcess);

    private static void DoOperation(BaseWriter writer, HookMode mode)
    {
        if (writer.Enabled && writer.Mode == mode)
            if (writer.Target?.IsActive() ?? false)
                    if (writer.Input?.IsActive() ?? false)
                    {
                        var value = writer.Input.GetReceivedEverPos(0);
                        if (value.VariantType == writer.ExpectedType || writer.ExpectedType == Variant.Type.Max)
                            if (!VariantExtension.ValueEqual(writer.LastWrite, value))
                                if (writer.Write(writer.Target, value))
                                    writer.LastWrite = value;          
                    }
    }
}
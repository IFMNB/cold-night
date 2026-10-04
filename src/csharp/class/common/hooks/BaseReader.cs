using ColdNight.src;
using Godot;

namespace ColdNight.src.common.hooks;

/// <summary>
/// Базовый класс для всех читателей данных, которые могут быть подключены к источникам данных и считывать их значения.
/// 
/// Наследники этого класса используются в системе "проводов" для получения данных из других объектов и передачи их в другие объекты.
/// </summary>
[GlobalClass] public abstract partial class BaseReader : Node, IWireSource, ITargetable<Node?>
{
    [Export] public bool Enabled {get;set;} = true;
    [Export] public HookMode Mode {get;set;} = HookMode.OnProcess;
    [Export] public WireOut? Output {get;set;}
    [Export] public Variant LastRead {get;set;}
    [Export] public Node? Target {get => RealTarget;set => RealTarget = value;}

    /// <summary>
    /// Реальная цель для чтения данных. Может быть установлена вручную или автоматически через <see cref="Target"/>.
    /// </summary>
    protected virtual Node? RealTarget {get;set;}

    /// <summary>
    /// Тип данных, которые ожидаются от `Target` при <see cref="Read"/>. Неподходящий тип данных будет игнорироваться и не будет записан в `LastRead` и `Output`.
    /// </summary>
    [Export] public Godot.Variant.Type ExpectedType {get;set;} = Godot.Variant.Type.Max;

    /// <summary>
    /// Попытка прочитать данные из `Target` и вернуть результат в `result`.
    /// </summary>
    /// <param name="Target"> Цель для чтения данных </param>
    /// <param name="result"> Результат чтения </param>
    /// <returns> Успешность операции чтения </returns>
    public abstract bool Read(Node Target, out Variant result);

    public override void _Ready () => DoOperation(this);
    public override void _EnterTree () => DoOperation(this);
    public override void _PhysicsProcess (double delta) => DoOperation(this);
    public override void _Process (double delta) => DoOperation(this);

    private static void DoOperation (BaseReader reader)
    {
        if (reader.Enabled && reader.Mode == HookMode.OnReady)
            if (IsInstanceValid(reader.Target))
                if (reader.Read(reader.Target, out var result))
                    if (result.VariantType == reader.ExpectedType || reader.ExpectedType == Variant.Type.Max)
                        reader.LastRead = result;
                

        if (IsInstanceValid(reader.Output))
            if (reader.Output.IsInsideTree())
                if (reader.LastRead.VariantType == reader.ExpectedType)
                    if (!VariantExtension.ValueEqual(reader.LastRead, reader.Output.Value))
                            reader.Output.Value = reader.LastRead;
    }               
}


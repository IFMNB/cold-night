using ColdNight.src;
using Godot;
using Godot.Collections;


/// <summary>
/// Объект для передачи сигналов того или иного рода без написания кода. По сути динамическая система транспортировки.
/// </summary>
[GlobalClass, Icon("res://addons/at-icons/node/node_graph_connection.svg")] public partial class Wire : Node {
    [Signal] public delegate void ReceivedEventHandler();
    [Signal] public delegate void EmittedEventHandler();

    [Export] public VariantExtension.OperationMode ReceiverMode = VariantExtension.OperationMode.Add;

    [Export] public Godot.Collections.Array<Wire> Receivers = [];
    
    /// <summary>
    /// Для инспектора. Позволяет вызвать передачу с последними аргументами провода.
    /// </summary>
    [Export] public bool RequireEmit = false;
    /// <summary>
    /// Если значение не совпало по типу с Default, то оно делает fallback на него.
    /// 
    /// <para>
    /// Если в Default такое значение Nil, то это поведение не будет выполнено и передастся то что есть
    /// </para>
    /// </summary>
    [Export] public bool FallbackToDefault = false;

    /// <summary>
    /// То что будет записано в LastReceive как только нода вообще появится
    /// </summary>
    [Export] public Array<Variant> DefaultReceive = [];
    [Export] public Array<Variant> DefaultEmit = [];


    protected Variant[] LastReceive = [];
    protected Variant[] LastWrite = [];

    protected bool ReceivedNow = false;
    protected bool WritedNow = false;

    public Godot.Collections.Array<Variant> GetReceivedEver () => [.. LastReceive];
    public Godot.Collections.Array<Variant> GetEmittedEver () => [.. LastWrite];

    public bool IsReceivedNow () => ReceivedNow;
    public bool IsEmittedNow () => WritedNow;

    public Variant GetReceivedEverPos (int pos)
    {
        if (pos >= 0 && pos < LastReceive.Length && LastReceive.GetValue(pos) is Variant variant)
            return variant;
        return default;
    }

    public Variant GetWritedEverPos (int pos)
    {
        
        if (pos >= 0 && pos < LastWrite.Length && LastWrite.GetValue(pos) is Variant variant)
            return variant;
        return default;
    }

    public bool GetReceivedNow (out Variant[] args)
    {
        args = LastReceive;
        return ReceivedNow;
    }

    public bool GetEmittedNow (out Variant[] args)
    {
        args = LastWrite;
        return WritedNow;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        ReceivedNow = false;
        WritedNow = false;

        if (RequireEmit)
        {
            Emit(LastWrite);
            RequireEmit = false;
        }
    }

    public override void _Ready()
    {
        base._Ready();

        // Default становится начальным значением "последнего" состояния
        LastReceive = [.. DefaultReceive];
        LastWrite = [.. DefaultEmit];
    }

    /// <summary>
    /// Подменяет аргументы, не совпавшие по типу с Default, на значение из Default.
    /// Числа (Int/Float) приводятся друг к другу. Недостающие аргументы добираются из Default.
    /// Если в Default по позиции Nil, аргумент остаётся как есть (а недостающий остаётся Nil).
    /// </summary>
    protected static Variant[] ApplyFallback (Wire wire, Variant[] args, Array<Variant> defaults)
    {
        if (!wire.FallbackToDefault || defaults.Count == 0)
            return args;

        int length = Mathf.Max(args.Length, defaults.Count);
        var result = new Variant[length];

        for (int i = 0; i < length; i++)
        {
            bool hasArg = i < args.Length;
            bool hasDef = i < defaults.Count;

            if (!hasArg)
            {
                result[i] = defaults[i];
                continue;
            }

            if (!hasDef || defaults[i].VariantType == Variant.Type.Nil)
            {
                result[i] = args[i];
                continue;
            }

            var arg = args[i];
            var def = defaults[i];

            if (arg.VariantType == def.VariantType)
                result[i] = arg;
            else if (arg.IsNumeric() && def.IsNumeric())
                result[i] = def.VariantType == Variant.Type.Float
                    ? Variant.From(arg.AsDouble())
                    : Variant.From(arg.AsInt64());
            else
                result[i] = def;
        }

        return result;
    }

    public bool Emit (params Variant[] args)
    {
        args = ApplyFallback(this, args, DefaultEmit);

        LastWrite = args;
        WritedNow = true;
        EmitSignalEmitted();

        if (Receivers.Count > 0)
        {
            foreach (var i in Receivers) Write(i, args);
            return true;
        }

        return false;
    }

    protected void Write (Wire Target, Variant[] args)
    {
        Target.ReceivedNow = true;
        Target.LastReceive = ApplyFallback(Target, args, Target.DefaultReceive);
        Target.EmitSignalReceived();
    }
}
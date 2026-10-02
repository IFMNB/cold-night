using System;
using System.Threading;
using System.Threading.Tasks;

using ColdNight.src.common;

using Godot;

using NuLua;
using NuLua.Luau;

namespace ColdNight.src.game.mods;

/// <summary>
/// Базовый Luau Runtime.
///
/// Не знает ничего о модах. Предоставляет:
/// - Luau VM;
/// - выполнение обычных chunk'ов;
/// - создание coroutine/thread;
/// - выполнение execution slices;
/// - timeout/cancellation;
/// - Process scheduling.
/// </summary>
[GlobalClass]
public partial class LuauRunner : Node, ISwitchable
{
    /// <summary>
    /// Luau VM.
    /// </summary>
    protected LuauState? Runtime { get; private set; }

    private LuauInterrupt? _interrupt;

    [Export]
    public bool Enabled { get; set; } = false;

    [Export]
    public bool Parallel { get; set; } = true;

    [Export(PropertyHint.Range, "1,240,1")]
    public byte Hertz { get; set; } = 40;

    [Export(PropertyHint.Range, "1,10000,1")]
    public int ExecutionTimeoutMs { get; set; } = 250;

    private readonly object _runtimeLock = new();

    private double _processAccumulator;

    private int _processScheduled;

    private bool _disposed;

    public override void _Ready()
    {
        Runtime = LuauState.Create();

        Runtime.OpenLibraries();

        _interrupt = new LuauInterrupt(
            Runtime
        );

        ConfigureRuntime(Runtime);
    }

    protected virtual void ConfigureRuntime(
        LuauState runtime)
    {
    }

    protected virtual void ProcessLuau(
        double delta)
    {
    }

    public override void _Process(
        double delta)
    {
        if (!Enabled || Hertz == 0)
            return;

        _processAccumulator += delta;

        double interval =
            1.0 / Hertz;

        if (_processAccumulator < interval)
            return;

        _processAccumulator %= interval;

        if (Parallel)
        {
            ScheduleParallelProcess(delta);
        }
        else
        {
            ExecuteProcess(delta);
        }
    }

    /// <summary>
    /// Выполняет обычный Luau chunk до его завершения.
    /// </summary>
    public LuaValue[] DoString(
        string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        lock (_runtimeLock)
        {
            ThrowIfDisposed();

            LuauState runtime =
                Runtime
                ?? throw new InvalidOperationException(
                    "Luau runtime has not been initialized."
                );

            LuauInterrupt interrupt =
                _interrupt
                ?? throw new InvalidOperationException(
                    "Luau interrupt has not been initialized."
                );

            int baseTop =
                runtime.GetTop();

            interrupt.Begin(
                ExecutionTimeoutMs
            );

            try
            {
                return runtime.DoString(
                    source
                );
            }
            catch (LuaException exception)
            {
                if (interrupt.TimedOut)
                {
                    throw new LuauRunnerTimeoutException(
                        ExecutionTimeoutMs,
                        exception
                    );
                }

                if (interrupt.Cancelled)
                {
                    throw new LuauRunnerCancelledException(
                        exception
                    );
                }

                throw;
            }
            finally
            {
                if (
                    interrupt.TimedOut ||
                    interrupt.Cancelled
                )
                {
                    LuauNative.ResetThread(
                        runtime
                    );
                }

                runtime.SetTop(
                    baseTop
                );

                interrupt.End();
            }
        }
    }

    /// <summary>
    /// Выполняет обычный chunk в отдельном worker.
    /// </summary>
    public Task<LuaValue[]> DoStringAsync(
        string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return Task.Run(
            () => DoString(source)
        );
    }

    /// <summary>
    /// Создаёт новый Luau coroutine/thread.
    ///
    /// Ownership остаётся у Runtime.
    /// </summary>
    protected LuauState CreateThread()
    {
        lock (_runtimeLock)
        {
            ThrowIfDisposed();

            LuauState runtime =
                Runtime
                ?? throw new InvalidOperationException(
                    "Luau runtime has not been initialized."
                );

            runtime.NewThread();

            LuauState thread =
                runtime.ToThread(-1);

            /*
             * ToThread() создал registry reference,
             * поэтому thread больше не нужен на stack Runtime.
             */
            runtime.Pop(1);

            return thread;
        }
    }

    /// <summary>
    /// Возобновляет coroutine.
    ///
    /// Возвращает:
    /// true  — coroutine завершилась;
    /// false — coroutine сделала yield и может быть продолжена.
    ///
    /// Timeout/cancellation превращаются в исключение.
    /// После timeout coroutine больше нельзя продолжить:
    /// она reset'ится.
    /// </summary>
    protected bool ResumeThread(
        LuauState thread)
    {
        lock (_runtimeLock)
        {
            ThrowIfDisposed();

            LuauInterrupt interrupt =
                _interrupt
                ?? throw new InvalidOperationException(
                    "Luau interrupt has not been initialized."
                );

            interrupt.Begin(
                ExecutionTimeoutMs
            );

            try
            {
                /*
                 * Resume(0) продолжает coroutine.
                 *
                 * При первом вызове это запускает загруженный chunk.
                 */
                thread.Resume(0);

                int status =
                    LuauNative.GetStatus(
                        thread
                    );

                if (status == LuauNative.LuaYield)
                {
                    /*
                     * Yielded values не являются частью API
                     * ModificationRunner, поэтому удаляем их.
                     */
                    thread.SetTop(0);

                    return false;
                }

                if (status == LuauNative.LuaOk)
                {
                    thread.SetTop(0);

                    return true;
                }

                return false;
            }
            catch (LuaException exception)
            {
                if (interrupt.TimedOut)
                {
                    /*
                     * LUA_BREAK оставляет coroutine в остановленном
                     * состоянии. Продолжать её нельзя.
                     */
                    LuauNative.ResetThread(
                        thread
                    );

                    thread.SetTop(0);

                    throw new LuauRunnerTimeoutException(
                        ExecutionTimeoutMs,
                        exception
                    );
                }

                if (interrupt.Cancelled)
                {
                    LuauNative.ResetThread(
                        thread
                    );

                    thread.SetTop(0);

                    throw new LuauRunnerCancelledException(
                        exception
                    );
                }

                throw;
            }
            finally
            {
                interrupt.End();
            }
        }
    }

    /// <summary>
    /// Освобождает coroutine.
    /// </summary>
    protected void DisposeThread(
        LuauState? thread)
    {
        if (thread == null)
            return;

        lock (_runtimeLock)
        {
            thread.Dispose();
        }
    }

    public void Cancel()
    {
        _interrupt?.Cancel();
    }

    private void ScheduleParallelProcess(
        double delta)
    {
        if (
            Interlocked.CompareExchange(
                ref _processScheduled,
                1,
                0
            ) != 0
        )
        {
            return;
        }

        ThreadPool.QueueUserWorkItem(
            _ =>
            {
                try
                {
                    ExecuteProcess(delta);
                }
                finally
                {
                    Volatile.Write(
                        ref _processScheduled,
                        0
                    );
                }
            }
        );
    }

    private void ExecuteProcess(
        double delta)
    {
        lock (_runtimeLock)
        {
            if (_disposed)
                return;

            if (!Enabled)
                return;

            ProcessLuau(delta);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(
                nameof(LuauRunner)
            );
        }
    }

    public override void _ExitTree()
    {
        _disposed = true;

        _interrupt?.Cancel();

        lock (_runtimeLock)
        {
            _interrupt?.Dispose();
            _interrupt = null;

            Runtime?.Dispose();
            Runtime = null;
        }

        base._ExitTree();
    }
}

/// <summary>
/// Luau execution превысил разрешённое время.
/// </summary>
[Serializable]
public sealed class LuauRunnerTimeoutException : Exception
{
    public int TimeoutMs { get; }

    public LuauRunnerTimeoutException(
        int timeoutMs,
        Exception inner)
        : base(
            $"Luau execution exceeded {timeoutMs} ms.",
            inner)
    {
        TimeoutMs = timeoutMs;
    }
}

/// <summary>
/// Luau execution был отменён.
/// </summary>
[Serializable]
public sealed class LuauRunnerCancelledException : Exception
{
    public LuauRunnerCancelledException(
        Exception inner)
        : base(
            "Luau execution was cancelled.",
            inner)
    {
    }
}
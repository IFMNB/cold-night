using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using NuLua.Interop.Luau;
using NuLua.Luau;

namespace ColdNight.src.game.mods;

internal static unsafe class LuauNative
{
    public const int LuaOk = 0;
    public const int LuaYield = 1;
    public const int LuaBreak = 6;

    public static int GetStatus(LuauState state)
    {
        return NativeMethods.lua_status(
            state.AsPointer()
        );
    }

    public static void ResetThread(LuauState state)
    {
        NativeMethods.lua_resetthread(
            state.AsPointer()
        );
    }
}

/// <summary>
/// Native interrupt bridge для Luau.
///
/// Единственный класс, которому требуется unsafe-код для timeout.
/// </summary>
internal unsafe sealed class LuauInterrupt : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void InterruptDelegate(
        lua_State* state,
        int gcState
    );

    private readonly LuauState _runtime;
    private readonly InterruptDelegate _delegate;

    private void* _previousInterrupt;

    private long _deadline = long.MaxValue;

    private int _executing;
    private int _cancelRequested;
    private int _timedOut;

    private bool _disposed;

    public LuauInterrupt(LuauState runtime)
    {
        _runtime = runtime;
        _delegate = Interrupt;

        Install();
    }

    public bool TimedOut =>
        Volatile.Read(ref _timedOut) != 0;

    public bool Cancelled =>
        Volatile.Read(ref _cancelRequested) != 0;

    /// <summary>
    /// Начинает контролируемый execution slice.
    /// </summary>
    public void Begin(int timeoutMs)
    {
        Volatile.Write(
            ref _cancelRequested,
            0
        );

        Volatile.Write(
            ref _timedOut,
            0
        );

        Volatile.Write(
            ref _executing,
            1
        );

        if (timeoutMs <= 0)
        {
            Volatile.Write(
                ref _deadline,
                long.MaxValue
            );

            return;
        }

        long timeoutTicks =
            (long)(
                timeoutMs *
                (double)Stopwatch.Frequency /
                1000.0
            );

        Volatile.Write(
            ref _deadline,
            Stopwatch.GetTimestamp() +
            timeoutTicks
        );
    }

    /// <summary>
    /// Завершает execution slice.
    /// </summary>
    public void End()
    {
        Volatile.Write(
            ref _executing,
            0
        );

        Volatile.Write(
            ref _deadline,
            long.MaxValue
        );

        Volatile.Write(
            ref _cancelRequested,
            0
        );
    }

    /// <summary>
    /// Запрашивает остановку текущего execution slice.
    /// </summary>
    public void Cancel()
    {
        Volatile.Write(
            ref _cancelRequested,
            1
        );
    }

    private void Install()
    {
        lua_Callbacks* callbacks =
            NativeMethods.lua_callbacks(
                _runtime.AsPointer()
            );

        _previousInterrupt =
            callbacks->interrupt;

        callbacks->interrupt =
            (void*)Marshal.GetFunctionPointerForDelegate(
                _delegate
            );
    }

    private void Uninstall()
    {
        lua_Callbacks* callbacks =
            NativeMethods.lua_callbacks(
                _runtime.AsPointer()
            );

        callbacks->interrupt =
            _previousInterrupt;

        _previousInterrupt = null;
    }

    private void Interrupt(
        lua_State* state,
        int gcState
    )
    {
        /*
         * Во время GC ничего не делаем.
         */
        if (gcState != -1)
            return;

        if (Volatile.Read(ref _executing) == 0)
            return;

        bool cancelled =
            Volatile.Read(ref _cancelRequested) != 0;

        long deadline =
            Volatile.Read(ref _deadline);

        bool timedOut =
            deadline != long.MaxValue &&
            Stopwatch.GetTimestamp() >= deadline;

        if (!cancelled && !timedOut)
            return;

        if (timedOut)
        {
            Volatile.Write(
                ref _timedOut,
                1
            );
        }

        NativeMethods.lua_break(state);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        Uninstall();
    }
}

internal static unsafe class NativeMethodsHelper
{
    public static void ResetThread(
        LuauState runtime
    )
    {
        NativeMethods.lua_resetthread(
            runtime.AsPointer()
        );
    }
}
using System;
using System.IO;

using Godot;

using NuLua.Luau;

namespace ColdNight.src.game.mods;

/// <summary>
/// Runtime конкретной модификации.
///
/// main.luau загружается один раз в отдельный Luau coroutine.
/// Далее coroutine возобновляется на каждом Process.
///
/// Основной контракт main.luau:
///
/// while true do
///     -- работа мода
///     coroutine.yield()
/// end
///
/// Один Resume соответствует одному execution slice.
/// </summary>
[GlobalClass]
public partial class ModificationRunner : LuauRunner
{
    [Export] public ModificationManifest? Manifest { get; set; }

    [Export] public bool AutoStart { get; set; } = true;

    public bool Started { get; private set; }
    public bool Finished { get; private set; }
    public bool Failed { get; private set; }
    public string? MainPath { get; private set; }

    private LuauState? _mainThread;

    public override void _Ready()
    {
        base._Ready();

        if (AutoStart)
            Start();
    }

    /// <summary>
    /// Загружает main.luau и запускает lifecycle модификации.
    ///
    /// Сам первый Resume произойдёт на Process.
    /// </summary>
    public void Start()
    {
        if (Started)
            return;

        if (Manifest == null)
        {
            Fail(
                "Manifest is not assigned."
            );

            return;
        }

        if (Manifest.ModMainType != ModificationRuntime.LuauMod)
        {
            Fail(
                $"Unsupported modification runtime " +
                $"'{Manifest.ModMainType}'."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(Manifest.ModPath))
        {
            Fail(
                "Manifest.ModPath is empty."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(Manifest.ModMain))
        {
            Fail(
                "Manifest.ModMain is empty."
            );

            return;
        }

        MainPath = ResolveMainPath(Manifest.ModPath,Manifest.ModMain);

        if (MainPath == null)
        {
            Failed = true;
            return;
        }

        string source;

        try
        {
            source = File.ReadAllText(
                MainPath
            );
        }
        catch (Exception exception)
        {
            Fail(
                $"Failed to read '{MainPath}': " +
                exception.Message
            );

            return;
        }

        try
        {
            _mainThread =
                CreateThread();

            _mainThread.LoadString(
                source,
                MainPath
            );

            Started = true;
            Finished = false;
            Failed = false;

            Enabled = true;

            GD.Print(
                $"ModificationRunner: loaded " +
                $"'{Manifest.ModName}' " +
                $"({Manifest.ModVersion})"
            );
        }
        catch (Exception exception)
        {
            _mainThread = null;

            Fail(
                $"Failed to load main of " +
                $"'{Manifest.ModName}': " +
                exception
            );
        }
    }

    /// <summary>
    /// Останавливает модификацию и освобождает её coroutine.
    /// </summary>
    public void Stop()
    {
        Enabled = false;

        DisposeThread(
            _mainThread
        );

        _mainThread = null;

        Started = false;
    }

    protected override void ProcessLuau(
        double delta)
    {
        _ = delta;

        LuauState? thread =
            _mainThread;

        if (!Started ||
            Finished ||
            Failed ||
            thread == null)
        {
            return;
        }

        try
        {
            bool finished =
                ResumeThread(thread);

            if (finished)
            {
                Finished = true;
                Enabled = false;

                GD.Print(
                    $"ModificationRunner: " +
                    $"'{Manifest?.ModName}' finished."
                );
            }
        }
        catch (LuauRunnerTimeoutException exception)
        {
            Failed = true;
            Enabled = false;

            GD.PushError(
                $"ModificationRunner: " +
                $"'{Manifest?.ModName}' timed out: " +
                exception.Message
            );

            DisposeThread(
                _mainThread
            );

            _mainThread = null;
        }
        catch (LuauRunnerCancelledException)
        {
            Failed = false;
            Enabled = false;

            DisposeThread(
                _mainThread
            );

            _mainThread = null;
        }
        catch (Exception exception)
        {
            Failed = true;
            Enabled = false;

            GD.PushError(
                $"ModificationRunner: " +
                $"'{Manifest?.ModName}' failed: " +
                exception
            );

            DisposeThread(
                _mainThread
            );

            _mainThread = null;
        }
    }

    private void Fail(
        string message)
    {
        Failed = true;
        Enabled = false;

        GD.PushError(
            $"ModificationRunner: " +
            $"{message}"
        );
    }

    private static string? ResolveMainPath(
        string modPath,
        string main)
    {
        string modRoot =
            Path.GetFullPath(
                modPath
            );

        string mainPath =
            Path.GetFullPath(
                Path.Combine(
                    modRoot,
                    main
                )
            );

        string relative =
            Path.GetRelativePath(
                modRoot,
                mainPath
            );

        if (
            relative == ".." ||
            relative.StartsWith(
                ".." +
                Path.DirectorySeparatorChar,
                StringComparison.Ordinal
            ) ||
            Path.IsPathRooted(relative)
        )
        {
            GD.PushError(
                $"ModificationRunner: main path " +
                $"escapes mod directory: '{main}'."
            );

            return null;
        }

        if (!File.Exists(mainPath))
        {
            GD.PushError(
                $"ModificationRunner: main file not found: " +
                $"'{mainPath}'."
            );

            return null;
        }

        return mainPath;
    }

    public override void _ExitTree()
    {
        Stop();

        base._ExitTree();
    }
}
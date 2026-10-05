using System;
using System.Text.Json;
using Godot;

namespace ColdNight.src.game.mods;

[GlobalClass, Icon("res://addons/at-icons/node/file_cog.svg")] public partial class ModificationManifest : Resource
{
    [Export] public ModificationStartOn ModType = ModificationStartOn.Global;
    [Export] public ModificationRuntime ModMainType = ModificationRuntime.LuauMod;
    [Export] public string ModName = string.Empty;
    [Export] public string ModVersion = string.Empty;
    [Export] public string ModMain = string.Empty;
    [Export] public ModificationRootType ModFrom = ModificationRootType.RootMods;

    /// <summary>
    /// Абсолютный путь к директории установленной модификации.
    ///
    /// Не читается из mod.json.
    /// Устанавливается ModificationLoader.
    /// </summary>

    [Export] public string ModPath { get; set; } = string.Empty;

    public ModificationManifest(JsonElement json)
    {
        ModType = ParseEnum<ModificationStartOn>(json, "type");
        ModMainType = ParseEnum<ModificationRuntime>(json, "run");
        ModName = ParseString(json, "name");
        ModVersion = ParseString(json, "version");
        ModMain = ParseString(json, "main");
    }

    private static string ParseString (JsonElement json, string Name) => json.GetProperty(Name).GetString() ?? throw new NullReferenceException($"{Name} isn't correct, can't read it");
    private static T ParseEnum<T>(JsonElement json, string name) where T : struct, Enum =>
        Enum.Parse<T>(
            json.GetProperty(name).GetString()
            ?? throw new NullReferenceException(
                $"{name} type isn't correct, can't read it"));}

/// <summary>
/// Определяет где и в какой ситуации запускать этот мод
/// </summary>
public enum ModificationStartOn
{
    /// <summary>
    /// Мод запускается при запуске игровой сессии на сервере.
    /// Выключается при выключении сервера.
    /// </summary>
    InGameServer,

    /// <summary>
    /// Мод запускается при запуске игровой сессии на клиенте.
    /// Выключается при отключении от сервера.
    /// </summary>
    InGameClient,
    /// <summary>
    /// Мод запускается при запуске игровой сессии на клиенте и сервере.
    /// Выключается при отключении от сервера или выходе с игровой сессии.
    /// </summary>
    InGameBoth,

    /// <summary>
    /// Мод запускается при запуске программы и никогда не выключается, даже
    /// если была игровая сессия.
    /// </summary>
    Global
}

/// <summary>
/// Определяет кто запускает этот мод
/// </summary>
public enum ModificationRuntime
{
    /// <summary>
    /// Главный файл мода <c>main</c> запускается как Luau
    /// </summary>
    LuauMod
}
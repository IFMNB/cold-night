using System;
using System.Collections.Generic;
using System.IO;
using Godot;

#if GODOT_WINDOWS
using Microsoft.Win32;
#endif

namespace ColdNight.src.game.mods;

/// <summary>
/// Утилитарный синглтон для работы с модификациями. Используется для поиска кандидатов
/// </summary>
public static class ModificationSearcher
{
    /// <summary>
    /// Возвращает корневые директории, в которых следует искать моды.
    /// </summary>
    public static IEnumerable<string> GetModsRoots(
        ModificationRootType rootType)
    {
        return rootType switch
        {
            ModificationRootType.RootMods  => GetRootModsRoots(),
            ModificationRootType.SteamMods => GetSteamModsRoots(),
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Возвращает директории с установленными модами.
    /// </summary>
    public static IEnumerable<string> GetMods(
        ModificationRootType rootType)
    {
        return rootType switch
        {
            ModificationRootType.RootMods  => GetRootMods(),
            ModificationRootType.SteamMods => GetSteamMods(),
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Корневые директории для обычных модов.
    ///
    /// res://mods  — моды, поставляемые вместе с игрой.
    /// user://mods — пользовательские моды.
    /// </summary>
    private static IEnumerable<string> GetRootModsRoots()
    {
        yield return ProjectSettings.GlobalizePath("res://mods");
        yield return ProjectSettings.GlobalizePath("user://mods");
    }

    private static IEnumerable<string> GetRootMods()
    {
        foreach (string root in GetRootModsRoots())
        {
            if (!Directory.Exists(root))
                continue;

            foreach (string modPath in Directory.EnumerateDirectories(root))
            {
                if (IsMod(modPath))
                    yield return modPath;
            }
        }
    }

    /// <summary>
    /// Возвращает корневые директории Steam Workshop.
    ///
    /// Обычно это:
    ///
    /// steamapps/workshop/content/
    ///
    /// внутри которой находятся:
    ///
    /// <SteamAppId>/<WorkshopItemId>/
    /// </summary>
    private static IEnumerable<string> GetSteamModsRoots()
    {
        foreach (string steamRoot in GetSteamRoots())
        {
            string workshopRoot = Path.Combine(
                steamRoot,
                "steamapps",
                "workshop",
                "content",
                ProjectSettings.GetSetting("global/steam_app_id", "0").ToString());

            if (Directory.Exists(workshopRoot))
                yield return workshopRoot;
        }
    }

    private static IEnumerable<string> GetSteamMods()
    {
        foreach (string workshopRoot in GetSteamModsRoots())
        {
            // content/<SteamAppId>/
            foreach (string appRoot in Directory.EnumerateDirectories(
                workshopRoot))
            {
                // content/<SteamAppId>/<WorkshopItemId>/
                foreach (string modPath in Directory.EnumerateDirectories(
                    appRoot))
                {
                    if (IsMod(modPath))
                        yield return modPath;
                }
            }
        }
    }

    /// <summary>
    /// Возвращает возможные корневые директории Steam.
    /// </summary>
    private static IEnumerable<string> GetSteamRoots()
    {
#if GODOT_WINDOWS
        foreach (string path in GetWindowsSteamRootsFromRegistry())
            yield return path;

        string pf86 = Environment.GetFolderPath(
            Environment.SpecialFolder.ProgramFilesX86);

        if (!string.IsNullOrEmpty(pf86))
            yield return Path.Combine(pf86, "Steam");

        string pf = Environment.GetFolderPath(
            Environment.SpecialFolder.ProgramFiles);

        if (!string.IsNullOrEmpty(pf))
            yield return Path.Combine(pf, "Steam");

#elif GODOT_LINUXBSD
        string home = System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.UserProfile);

        if (!string.IsNullOrEmpty(home))
        {
            // Обычная установка Steam.
            yield return Path.Combine(
                home,
                ".steam",
                "steam");

            // XDG.
            yield return Path.Combine(
                home,
                ".local",
                "share",
                "Steam");

            // Steam Flatpak.
            yield return Path.Combine(
                home,
                ".var",
                "app",
                "com.valvesoftware.Steam",
                ".local",
                "share",
                "Steam");
        }

#elif GODOT_MACOS
        string home = Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile);

        if (!string.IsNullOrEmpty(home))
        {
            yield return Path.Combine(
                home,
                "Library",
                "Application Support",
                "Steam");
        }
#endif
    }

#if GODOT_WINDOWS

    /// <summary>
    /// Возвращает пути Steam из HKCU.
    /// </summary>
    private static IEnumerable<string> GetWindowsSteamRootsFromRegistry()
    {
        string? steamPath = TryReadRegistryString(
            @"Software\Valve\Steam",
            "SteamPath");

        if (!string.IsNullOrWhiteSpace(steamPath))
        {
            string normalized = NormalizePath(steamPath);

            if (Directory.Exists(normalized))
                yield return normalized;
        }

        string? steamExe = TryReadRegistryString(
            @"Software\Valve\Steam",
            "SteamExe");

        if (!string.IsNullOrWhiteSpace(steamExe))
        {
            string normalized = NormalizePath(steamExe);

            string? directory = Path.GetDirectoryName(normalized);

            if (!string.IsNullOrEmpty(directory) &&
                Directory.Exists(directory))
            {
                yield return directory;
            }
        }
    }

    /// <summary>
    /// Безопасно читает строковое значение из HKCU.
    /// </summary>
    private static string? TryReadRegistryString(
        string subKey,
        string valueName)
    {
        try
        {
            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    subKey,
                    writable: false);

            return key?.GetValue(valueName) as string;
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizePath(string path)
    {
        return path
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .TrimEnd(Path.DirectorySeparatorChar);
    }

#endif

    /// <summary>
    /// Проверяет, является ли директория модом.
    /// Наличие mod.json является признаком установленного мода.
    /// </summary>
    private static bool IsMod(string modPath)
    {
        return File.Exists(GetModJsonPath(modPath));
    }

    /// <summary>
    /// Возвращает путь к mod.json.
    /// </summary>
    public static string GetModJsonPath(string modPath)
    {
        return Path.Combine(modPath, ModificationLoader.ManifestFileName);
    }

    /// <summary>
    /// Возвращает путь к модификации внутри корня.
    /// </summary>
    public static string GetModPath(
        string root,
        string name)
    {
        return Path.Combine(root, name);
    }
}

/// <summary>
/// Источник, из которого выполняется поиск модов.
/// </summary>
public enum ModificationRootType
{
    /// <summary>
    /// Моды из:
    ///
    /// res://mods/
    /// user://mods/
    /// </summary>
    RootMods,

    /// <summary>
    /// Моды из Steam Workshop.
    /// </summary>
    SteamMods
}
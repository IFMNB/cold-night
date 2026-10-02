using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace ColdNight.src.game.mods;

/// <summary>
/// Загружает манифесты модификаций, найденных <see cref="ModificationSearcher"/>,
/// и хранит их в памяти. Сам по себе код мода не исполняет — только читает
/// <c>mod.json</c> и превращает его в <see cref="ModificationManifest"/>.
/// </summary>
public static class ModificationLoader
{
    public const string ManifestFileExtension = ".json";
    /// <summary>
    /// Имя файла манифеста внутри каталога мода.
    /// </summary>
    public const string ManifestFileName = $"mod{ManifestFileExtension}";

    /// <summary>
    /// Загружает все манифесты из указанного источника модов.
    /// </summary>
    public static IEnumerable<ModificationManifest> LoadAll(
        ModificationRootType rootType)
    {
        foreach (string modPath in ModificationSearcher.GetMods(rootType))
        {
            ModificationManifest? manifest = TryLoad(modPath);
            if (manifest is not null)
                yield return manifest;
        }
    }

    /// <summary>
    /// Загружает все манифесты из всех известных источников.
    /// Если один и тот же мод (по <c>ModName</c>) найден в нескольких источниках,
    /// приоритет имеет последний прочитанный.
    /// </summary>
    public static IReadOnlyDictionary<string, ModificationManifest> LoadAllUnique()
    {
        var result = new Dictionary<string, ModificationManifest>(StringComparer.Ordinal);

        foreach (ModificationRootType rootType in Enum.GetValues<ModificationRootType>())
        {
            foreach (ModificationManifest manifest in LoadAll(rootType))
            {
                if (string.IsNullOrWhiteSpace(manifest.ModName))
                    continue;

                result[manifest.ModName] = manifest;
            }
        }

        return result;
    }

    /// <summary>
    /// Пытается загрузить манифест из конкретной директории мода.
    /// Возвращает <c>null</c> и пишет предупреждение, если манифест отсутствует
    /// или повреждён.
    /// </summary>
    public static ModificationManifest? TryLoad(string modPath)
    {
        if (string.IsNullOrWhiteSpace(modPath) || !Directory.Exists(modPath))
        {
            GD.PushWarning($"ModificationLoader: mod directory not found: '{modPath}'");
            return null;
        }

        string jsonPath = ModificationSearcher.GetModJsonPath(modPath);

        if (!File.Exists(jsonPath))
        {
            GD.PushWarning($"ModificationLoader: manifest missing: '{jsonPath}'");
            return null;
        }

        try
        {
            return LoadFromFile(jsonPath);
        }
        catch (Exception ex)
        {
            GD.PushError($"ModificationLoader: failed to load '{jsonPath}': {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Читает манифест из файла <c>mod.json</c>.
    /// </summary>
    public static ModificationManifest LoadFromFile(string jsonPath)
    {
        string text = File.ReadAllText(jsonPath);

        using var document = JsonDocument.Parse(text, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("mod.json root must be a JSON object.");

        ModificationManifest manifest = new(document.RootElement);

        // ModFrom — поле, которое конструктор не читает из JSON (оно задаётся
        // вызывающей стороной), поэтому по умолчанию оставляем RootMods.
        // Источник можно скорректировать отдельно, см. LoadWithSource.
        return manifest;
    }

    /// <summary>
    /// То же, что <see cref="LoadFromFile"/>, но с явным источником мода.
    /// </summary>
    public static ModificationManifest LoadFromFile(
        string jsonPath,
        ModificationRootType source)
    {
        ModificationManifest manifest = LoadFromFile(jsonPath);

        manifest.ModFrom = source;
        manifest.ModPath =
            Path.GetDirectoryName(
                Path.GetFullPath(jsonPath)
            ) ?? string.Empty;

        return manifest;
    }

    /// <summary>
    /// Загружает манифест из директории мода с явным источником.
    /// </summary>
    public static ModificationManifest? TryLoad(
        string modPath,
        ModificationRootType source)
    {
        ModificationManifest? manifest = TryLoad(modPath);
        if (manifest is null)
            return null;

        manifest.ModFrom = source;
        return manifest;
    }

    /// <summary>
    /// Загружает манифесты всех модов из указанного источника,
    /// проставляя корректный <see cref="ModificationManifest.ModFrom"/>.
    /// </summary>
    public static IEnumerable<ModificationManifest> LoadAllWithSource(
        ModificationRootType rootType)
    {
        foreach (string modPath in ModificationSearcher.GetMods(rootType))
        {
            ModificationManifest? manifest = TryLoad(modPath, rootType);
            if (manifest is not null)
                yield return manifest;
        }
    }

    /// <summary>
    /// Собирает словарь "имя мода → манифест" для конкретного источника.
    /// </summary>
    public static IReadOnlyDictionary<string, ModificationManifest> LoadIndex(
        ModificationRootType rootType)
    {
        var result = new Dictionary<string, ModificationManifest>(StringComparer.Ordinal);

        foreach (ModificationManifest manifest in LoadAllWithSource(rootType))
        {
            if (string.IsNullOrWhiteSpace(manifest.ModName))
            {
                GD.PushWarning($"ModificationLoader: mod without name skipped (from {manifest.ModFrom}).");
                continue;
            }

            if (result.ContainsKey(manifest.ModName))
            {
                GD.PushWarning(
                    $"ModificationLoader: duplicate mod name '{manifest.ModName}' " +
                    $"in source {rootType}, overwriting previous entry.");
            }

            result[manifest.ModName] = manifest;
        }

        return result;
    }
}
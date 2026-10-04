using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

internal static class Generator
{
    public static void GenerateGodotNodes(
        string godotSharpPath,
        string metadataName,
        string metadataValueName,
        string outputDirectory = "./target")
    {
        var references = new[]
        {
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location),

            MetadataReference.CreateFromFile(
                typeof(Enumerable).Assembly.Location),

            MetadataReference.CreateFromFile(
                godotSharpPath)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "GodotGenerator",
            syntaxTrees: [],
            references: references);

        var godotObject =
            compilation.GetTypeByMetadataName(metadataName)
            ?? throw new Exception(
                $"{metadataName} not found.");

        var valueType =
            compilation.GetTypeByMetadataName(metadataValueName)
            ?? throw new Exception(
                $"{metadataValueName} not found.");

        if (godotObject.TypeKind != TypeKind.Class)
            throw new Exception(
                $"{metadataName} is not a class.");

        Console.WriteLine(
            $"{metadataName} found with {godotObject.GetMembers().Length} members, generating properties for {metadataValueName}...");

        GenerateEnumGodotProperties(
            godotObject,
            valueType,
            outputDirectory);
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(
        INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;

            foreach (var nested in GetNestedTypes(type))
                yield return nested;
        }

        foreach (var child in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(child))
                yield return type;
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(
        INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;

            foreach (var child in GetNestedTypes(nested))
                yield return child;
        }
    }

    private static void GenerateEnumGodotProperties(
        INamedTypeSymbol godotObject,
        ITypeSymbol valueType,
        string OutputDirectory)
    {
        var properties = GetProperties(godotObject)
            .Where(property =>
                property.GetMethod is not null &&
                property.SetMethod is not null &&
                SymbolEqualityComparer.Default.Equals(
                    property.Type,
                    valueType))
            .OrderBy(property => property.Name)
            .ToArray();

        if (properties.Length == 0)
            return;

        var source = new StringBuilder();

        source.AppendLine("using Godot;");
        source.AppendLine();
        source.AppendLine("namespace ColdNight.src.generated;");
        source.AppendLine();
        source.AppendLine($"public enum {godotObject.Name}{valueType.Name}Properties : int");
        source.AppendLine("{");
        foreach (var property in properties) source.AppendLine($"{property.Name},");
        source.AppendLine("}");

        WriteToFile($"{godotObject.Name}{valueType.Name}Properties.cs",source,OutputDirectory);
    }

    private static IEnumerable<IPropertySymbol> GetProperties(
        INamedTypeSymbol type)
    {
        var seen = new HashSet<string>();

        for (var current = type;
             current is not null;
             current = current.BaseType)
        {
            foreach (var property in current
                .GetMembers()
                .OfType<IPropertySymbol>())
            {
                if (!seen.Add(property.Name))
                    continue;

                yield return property;
            }
        }
    }

    private static void WriteToFile(
        string fileName,
        StringBuilder content,
        string OutputDirectory)
    {
        Directory.CreateDirectory(
            OutputDirectory);

        string path = Path.Combine(
            OutputDirectory,
            fileName);

        File.WriteAllText(
            path,
            content.ToString());

        Console.WriteLine(
            $"Generated: {path}");
    }
}
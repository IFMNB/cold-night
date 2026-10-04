using System;
using System.IO;
using System.Text.Json;

internal static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 3)
            throw new ArgumentException(
                "Usage: CodeGenerator <GodotSharp.dll> <ClassType.json> <ValueType.json>");

        string godotSharpPath = args[0];
        string classTypePath = args[1];
        string valueTypePath = args[2];
        string outputDirectory = args[3];

        string[] classTypes = ReadTypeNames(classTypePath);
        string[] valueTypes = ReadTypeNames(valueTypePath);

        foreach (string classType in classTypes)
        {
            foreach (string valueType in valueTypes)
            {
                Generator.GenerateGodotNodes(
                    godotSharpPath,
                    classType,
                    valueType, 
                    outputDirectory);
            }
        }
    }

    private static string[] ReadTypeNames(string path)
    {
        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<string[]>(json)
            ?? throw new InvalidOperationException(
                $"Failed to read type list from '{path}'.");
    }
}
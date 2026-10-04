# Cold Night Code Generator

A build-time C# source generator utility for the Cold Night Godot project.

The generator uses **Roslyn** to inspect `GodotSharp.dll`, resolve Godot C# types, and generate strongly typed property enums for selected Godot classes and value types.

## Configuration

### Class types

`GenClassType.json` contains the exact C# metadata names of the classes that should be processed.

Example:

```json
[
    "Godot.Node",
    "Godot.Node3D",
    "Godot.RigidBody3D"
]
```

The classes are treated as explicit targets.

The generator does **not** automatically enumerate all descendants of the supplied class.

### Value types

`GenValueType.json` contains the exact metadata names of the property types to search for.

Example:

```json
[
    "System.Boolean",
    "System.String",
    "Godot.StringName",
    "Godot.NodePath",
    "Godot.Vector3",
    "Godot.Transform3D"
]
```

Both .NET types and Godot types can be used.

## Generated code

Generated files use the namespace:

```csharp
namespace ColdNight.src.generated;
```

A generated enum looks like:

```csharp
using Godot;

namespace ColdNight.src.generated;

public enum Node3DVector3Properties : int
{
    Position,
    Rotation,
    Scale,
}
```

The enum contains the names of the matching properties.

## Run

The generator expects four arguments:

```text
CodeGenerator <GodotSharp.dll> <ClassType.json> <ValueType.json> <OutputDirectory>
```

The output directory is created automatically when necessary.

The generator is intended to be executed automatically from the main Cold Night project.

The main project resolves the actual `GodotSharp.dll` reference and passes it to the generator together with the JSON configuration files and output directory.

If you have Linux with fish, then you can use run_linux_fish in /auto .
Generated files are build artifacts and can be placed in a dedicated directory such as:

```text
src/csharp/auto/build/
```

MSBuild cleans up the directory itself from previous versions
The JSON metadata files should remain tracked because they define the generator's input.

## Requirements

* .NET 8
* Roslyn:

  * `Microsoft.CodeAnalysis.Common`
  * `Microsoft.CodeAnalysis.CSharp`
* Godot 4 .NET
* A compatible `GodotSharp.dll`

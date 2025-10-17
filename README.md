# 🧩 ReplaceObsoleteAnalyzer

**ReplaceObsoleteAnalyzer** is a custom **.NET Roslyn Analyzer and Code Fix Provider** that detects usages of obsolete members (marked with `[Obsolete]`) and offers automatic replacements when a suggested alternative is provided.

---

## 🚀 Overview

This analyzer helps maintain cleaner and more maintainable code by identifying usages of deprecated members and replacing them with the recommended new members as suggested in the `[Obsolete]` attribute message.

For example:

```csharp
[Obsolete("Use NewProperty instead")]
public string OldProperty { get; set; }
```

If your code uses:

```csharp
var x = obj.OldProperty;
```

The analyzer reports a diagnostic and provides a one-click fix to replace it with:

```csharp
var x = obj.NewProperty;
```

---

## 📦 Features

✅ Detects usage of `[Obsolete("Use X instead")]` members  
✅ Reports a clear diagnostic message (`ROB001`)  
✅ Provides an automatic code fix (“Replace with ‘X’”)  
✅ Fully compatible with Visual Studio and `dotnet build`  
✅ Lightweight and supports concurrent execution  

---

## 🧠 How It Works

### Analyzer (`ReplaceObsoleteAnalyzer`)
- Listens for `IdentifierNameSyntax` nodes.
- Checks if the symbol is marked `[Obsolete]`.
- Extracts the message (e.g., `"Use NewProp instead"`).
- Reports a diagnostic (`ROB001`).

### Code Fix Provider (`ReplaceObsoleteCodeFixProvider`)
- Parses the obsolete message to find the replacement (e.g., `"Use NewProp instead"` → `NewProp`).
- Registers a **code action** “Replace with ‘NewProp’”.
- Automatically replaces the identifier in your source code.

---

## 🧰 Example

### ❌ Before

```csharp
public class Sample
{
    [Obsolete("Use NewValue instead")]
    public int OldValue { get; set; }

    public int NewValue { get; set; }

    public void Test()
    {
        var x = OldValue;
    }
}
```

### ✅ After Code Fix

```csharp
public void Test()
{
    var x = NewValue;
}
```

---

## ⚙️ Installation

You can install **ReplaceObsoleteAnalyzer** via NuGet (once published):

```bash
dotnet add package ReplaceObsolete
```

Or manually include the `.nupkg` as an Analyzer reference:

1. Right-click your project → **Add → Analyzer...**
2. Select `ReplaceObsoleteAnalyzer.dll` or the `.nupkg` file.
3. Build your project — diagnostics will automatically appear in Visual Studio or `dotnet build`.

---

## 🧾 Diagnostic Info

| ID | Category | Severity | Message Format |
|----|-----------|-----------|----------------|
| ROB001 | Maintainability | Info | Member '{0}' is obsolete. Use '{1}' instead. |

---

## 🧩 Project Structure

```
ReplaceObsoleteAnalyzer/
│
├── ReplaceObsolete/                   # Analyzer
│   ├── ReplaceObsoleteAnalyzer.cs
│   └── Resources.resx
│
├── ReplaceObsolete.CodeFixes/         # CodeFix provider
│   └── ReplaceObsoleteCodeFixProvider.cs
│
├── ReplaceObsolete.Package/           # NuGet packaging
│   └── ReplaceObsolete.Package.csproj
│
├── ReplaceObsolete.Test/              # Analyzer and CodeFix tests
│   └── ReplaceObsoleteAnalyzerTests.cs
│
├── LICENSE
└── README.md
```

---

## 🧩 NuGet Packaging Details

The packaging project (`ReplaceObsolete.Package.csproj`) is configured to:

- Include both analyzer and code fix assemblies under `analyzers/dotnet/cs`
- Prevent redundant build outputs
- Mark as a development dependency
- Support .NET Standard 2.0 for maximum compatibility

Example configuration:

```xml
<Target Name="_AddAnalyzersToOutput">
  <ItemGroup>
    <TfmSpecificPackageFile Include="$(OutputPath)\ReplaceObsolete.dll" PackagePath="analyzers/dotnet/cs" />
    <TfmSpecificPackageFile Include="$(OutputPath)\ReplaceObsolete.CodeFixes.dll" PackagePath="analyzers/dotnet/cs" />
  </ItemGroup>
</Target>
```

---

## 🧑‍💻 Development

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (with **Roslyn SDK** workload)

### Build and Test
```bash
dotnet build
dotnet test
```

### Create NuGet Package
```bash
dotnet pack ReplaceObsolete.Package/ReplaceObsolete.Package.csproj -c Release
```

The package will be located in:
```
ReplaceObsolete.Package/bin/Release/
```

---

## 🧠 Notes

- The analyzer only applies to messages starting with `"Use "`.  
  (e.g., `[Obsolete("Use NewValue instead")]`)
- If the obsolete message does not follow this pattern, the diagnostic will still appear but no code fix will be offered.
- It supports concurrent execution and skips generated code.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!  
Please open an issue or submit a pull request.

---

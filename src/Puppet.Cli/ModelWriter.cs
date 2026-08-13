using System.Text.Json;
using Puppet.Core;

namespace Puppet.Cli;

/// <summary>
/// Reads/writes model.json using the frozen camelCase field names from
/// docs/spec.md section 5.
/// </summary>
public static class ModelWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static void Write(ModelDocument document, string outPath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(outPath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outPath, JsonSerializer.Serialize(document, Options));
    }

    public static ModelDocument Read(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ModelDocument>(json, Options)
            ?? throw new InvalidOperationException($"Could not parse existing model at '{path}'.");
    }
}

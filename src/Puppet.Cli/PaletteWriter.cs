using System.Text.Json;
using Puppet.Core;

namespace Puppet.Cli;

/// <summary>Writes palette.json using the same camelCase convention as ModelWriter.</summary>
public static class PaletteWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static void Write(PaletteDocument document, string outPath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(outPath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outPath, JsonSerializer.Serialize(document, Options));
    }
}

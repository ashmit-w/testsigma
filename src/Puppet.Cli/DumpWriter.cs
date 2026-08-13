using System.Text.Json;
using Puppet.Core;

namespace Puppet.Cli;

public static class DumpWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public static void Write(ElementNode root, string processName, string outPath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(outPath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var document = new
        {
            processName,
            dumpedAt = DateTime.UtcNow,
            root,
        };

        File.WriteAllText(outPath, JsonSerializer.Serialize(document, Options));
    }
}

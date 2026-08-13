using Puppet.Core;

namespace Puppet.Cli;

/// <summary>
/// puppet palette --model &lt;path.json&gt; --out &lt;path.json&gt;
/// </summary>
public static class PaletteCommand
{
    public static Task<int> Run(string[] args)
    {
        string? modelPath = null;
        string? outPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--model":
                    modelPath = NextValue(args, ref i, "--model");
                    break;
                case "--out":
                    outPath = NextValue(args, ref i, "--out");
                    break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return Task.FromResult(1);
            }
        }

        if (string.IsNullOrEmpty(modelPath))
        {
            Console.Error.WriteLine("--model is required.");
            return Task.FromResult(1);
        }

        if (string.IsNullOrEmpty(outPath))
        {
            Console.Error.WriteLine("--out is required.");
            return Task.FromResult(1);
        }

        ModelDocument model;
        try
        {
            model = ModelWriter.Read(modelPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not read model: {ex.Message}");
            return Task.FromResult(1);
        }

        var palette = BlockGenerator.Generate(model);
        PaletteWriter.Write(palette, outPath);
        Console.WriteLine($"Wrote {outPath} ({palette.Blocks.Count} blocks)");
        return Task.FromResult(0);
    }

    private static string NextValue(string[] args, ref int i, string flag)
    {
        if (i + 1 >= args.Length)
        {
            throw new ArgumentException($"{flag} requires a value.");
        }

        return args[++i];
    }
}

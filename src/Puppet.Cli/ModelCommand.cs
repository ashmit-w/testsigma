using Puppet.Core;

namespace Puppet.Cli;

/// <summary>
/// puppet model --process &lt;name&gt; --out &lt;path.json&gt; [--merge] [--pid &lt;n&gt;]
/// </summary>
public static class ModelCommand
{
    public static async Task<int> Run(string[] args)
    {
        string? processName = null;
        int? pid = null;
        string? outPath = null;
        var merge = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--process":
                    processName = NextValue(args, ref i, "--process");
                    break;
                case "--pid":
                    pid = int.Parse(NextValue(args, ref i, "--pid"));
                    break;
                case "--out":
                    outPath = NextValue(args, ref i, "--out");
                    break;
                case "--merge":
                    merge = true;
                    break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return 1;
            }
        }

        if (string.IsNullOrEmpty(processName) && pid is null)
        {
            Console.Error.WriteLine("--process or --pid is required.");
            return 1;
        }

        if (string.IsNullOrEmpty(outPath))
        {
            Console.Error.WriteLine("--out is required.");
            return 1;
        }

        using var session = new AutomationSession();
        ModelDocument fresh;
        try
        {
            fresh = await session.BuildModelAsync(processName, pid);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Model build failed: {ex.Message}");
            return 1;
        }

        var final = fresh;
        if (merge && File.Exists(outPath))
        {
            var existing = ModelWriter.Read(outPath);
            final = ModelMerger.Merge(existing, fresh);
        }

        ModelWriter.Write(final, outPath);
        Console.WriteLine($"Wrote {outPath} ({final.Elements.Count} elements, {final.Coverage.Unexplored.Count} unexplored)");
        return 0;
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

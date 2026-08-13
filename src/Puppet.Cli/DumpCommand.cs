using Puppet.Core;

namespace Puppet.Cli;

/// <summary>
/// puppet dump --process &lt;name&gt; --out &lt;path.json&gt; [--summary]
/// </summary>
public static class DumpCommand
{
    public static async Task<int> Run(string[] args)
    {
        string? processName = null;
        string? outPath = null;
        var summary = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--process":
                    processName = NextValue(args, ref i, "--process");
                    break;
                case "--out":
                    outPath = NextValue(args, ref i, "--out");
                    break;
                case "--summary":
                    summary = true;
                    break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return 1;
            }
        }

        if (string.IsNullOrEmpty(processName))
        {
            Console.Error.WriteLine("--process is required.");
            return 1;
        }

        if (!summary && string.IsNullOrEmpty(outPath))
        {
            Console.Error.WriteLine("--out is required unless --summary is passed.");
            return 1;
        }

        using var session = new AutomationSession();
        ElementNode root;
        try
        {
            root = await session.DumpTreeAsync(processName);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Dump failed: {ex.Message}");
            return 1;
        }

        if (summary)
        {
            SummaryPrinter.Print(root);
        }
        else
        {
            DumpWriter.Write(root, processName, outPath!);
            Console.WriteLine($"Wrote {outPath}");
        }

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

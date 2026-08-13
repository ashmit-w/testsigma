using Puppet.Core;

namespace Puppet.Cli;

/// <summary>
/// puppet session --exe path\to\App.exe
///
/// Starts an AppSession and drops into a REPL for testing it without the
/// editor: type an element id to click it and rescan, "reset" to restart
/// the app, "quit" to exit.
/// </summary>
public static class SessionCommand
{
    public static async Task<int> Run(string[] args)
    {
        string? exePath = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--exe":
                    exePath = NextValue(args, ref i, "--exe");
                    break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return 1;
            }
        }

        if (string.IsNullOrEmpty(exePath))
        {
            Console.Error.WriteLine("--exe is required.");
            return 1;
        }

        using var session = new AppSession();
        ModelDocument model;
        try
        {
            model = await session.StartAsync(exePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Start failed: {ex.Message}");
            return 1;
        }

        PrintPalette(model);

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            if (line == "quit")
            {
                return 0;
            }

            if (line == "reset")
            {
                model = await session.ResetAsync();
                PrintPalette(model);
                continue;
            }

            var target = model.Elements.FirstOrDefault(e => e.Id == line);
            if (target == null)
            {
                Console.WriteLine($"No element with id '{line}' in the current model. Try an id from the list above, \"reset\", or \"quit\".");
                continue;
            }

            var action = target.Patterns.Contains("Toggle") && !target.Patterns.Contains("Invoke")
                ? ActionKind.Toggle
                : ActionKind.Invoke;

            var flow = new Flow
            {
                Steps = [new FlowStep { Description = line, TargetPath = target.Path, Action = new ActionArgs { Kind = action } }],
            };

            var replay = await session.ReplayAsync(flow);
            var step = replay.StepResults[0];
            Console.WriteLine(step.Status == StepStatus.Passed
                ? $"OK via {step.Mechanism} (confidence {step.Confidence})"
                : $"FAILED: {step.Message ?? step.FailureCause?.ToString() ?? "no mechanism succeeded"}");

            model = replay.Model;
            PrintPalette(model);
        }
    }

    private static void PrintPalette(ModelDocument model)
    {
        Console.WriteLine($"-- {model.AppTitle} ({model.Elements.Count} elements) --");
        foreach (var element in model.Elements.Where(e => e.Mechanism != null))
        {
            var name = element.Name ?? element.AutomationId ?? "(unnamed)";
            Console.WriteLine($"  {element.Id}  {element.ControlType,-16} {name}");
        }
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

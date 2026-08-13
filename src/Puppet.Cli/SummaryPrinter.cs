using Puppet.Core;

namespace Puppet.Cli;

/// <summary>
/// Prints a flat controlType / automationId / name / patterns table for
/// the whole dumped tree, depth-first.
/// </summary>
public static class SummaryPrinter
{
    public static void Print(ElementNode root)
    {
        var rows = new List<(string ControlType, string AutomationId, string Name, string Patterns)>();
        Flatten(root, rows);

        var controlTypeWidth = Math.Max("controlType".Length, rows.Max(r => r.ControlType.Length));
        var automationIdWidth = Math.Max("automationId".Length, rows.Max(r => r.AutomationId.Length));
        var nameWidth = Math.Max("name".Length, rows.Max(r => r.Name.Length));

        PrintRow("controlType", "automationId", "name", "patterns", controlTypeWidth, automationIdWidth, nameWidth);
        foreach (var row in rows)
        {
            PrintRow(row.ControlType, row.AutomationId, row.Name, row.Patterns, controlTypeWidth, automationIdWidth, nameWidth);
        }
    }

    private static void Flatten(ElementNode node, List<(string, string, string, string)> rows)
    {
        rows.Add((node.ControlType, node.AutomationId ?? "", node.Name ?? "", string.Join(",", node.Patterns)));
        foreach (var child in node.Children)
        {
            Flatten(child, rows);
        }
    }

    private static void PrintRow(string controlType, string automationId, string name, string patterns,
        int controlTypeWidth, int automationIdWidth, int nameWidth)
    {
        Console.WriteLine(
            $"{controlType.PadRight(controlTypeWidth)}  {automationId.PadRight(automationIdWidth)}  {name.PadRight(nameWidth)}  {patterns}");
    }
}

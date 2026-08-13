using FlaUI.Core.AutomationElements;

namespace Puppet.Core;

/// <summary>
/// Walks a live (uncached) element tree down a structural path recorded
/// by ModelBuilder, to recover the actual element behind a model id.
/// Segment naming must mirror ModelBuilder.BuildChildSegments exactly.
/// </summary>
public static class ElementPathResolver
{
    public static AutomationElement? Resolve(AutomationElement root, IReadOnlyList<string> path)
    {
        var current = root;
        for (var i = 1; i < path.Count; i++)
        {
            var next = MatchSegment(current.FindAllChildren(), path[i]);
            if (next == null)
            {
                return null;
            }

            current = next;
        }

        return current;
    }

    private static AutomationElement? MatchSegment(AutomationElement[] children, string segment)
    {
        var typeNames = children.Select(GetControlTypeName).ToArray();
        var counts = typeNames.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
        var running = new Dictionary<string, int>();

        for (var i = 0; i < children.Length; i++)
        {
            var type = typeNames[i];
            var candidate = type;
            if (counts[type] > 1)
            {
                running[type] = running.GetValueOrDefault(type) + 1;
                candidate = $"{type}[{running[type]}]";
            }

            if (candidate == segment)
            {
                return children[i];
            }
        }

        return null;
    }

    private static string GetControlTypeName(AutomationElement element) =>
        element.Properties.ControlType.ValueOrDefault.ToString();
}

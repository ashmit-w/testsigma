namespace Puppet.Core;

/// <summary>
/// Structural coverage detection that works purely off a persisted
/// elements list (parent/child inferred from Path), so it can be rerun
/// after a merge without needing a live UIA tree.
/// </summary>
public static class CoverageDetector
{
    public static List<UnexploredContainer> DetectUnselectedTabPages(IReadOnlyList<ModelElement> elements)
    {
        var result = new List<UnexploredContainer>();

        foreach (var container in elements.Where(e => e.ControlType == "Tab"))
        {
            var children = elements.Where(e => IsDirectChild(container, e)).ToList();
            var tabItemCount = children.Count(c => c.ControlType == "TabItem");
            var realizedPaneCount = children.Count(c => c.ControlType == "Pane");

            if (tabItemCount > realizedPaneCount)
            {
                result.Add(new UnexploredContainer
                {
                    ContainerId = container.Id,
                    ContainerType = container.ControlType,
                    Reason = "UnselectedTabPage",
                });
            }
        }

        return result;
    }

    private static bool IsDirectChild(ModelElement parent, ModelElement candidate)
    {
        if (candidate.Path.Count != parent.Path.Count + 1)
        {
            return false;
        }

        for (var i = 0; i < parent.Path.Count; i++)
        {
            if (candidate.Path[i] != parent.Path[i])
            {
                return false;
            }
        }

        return true;
    }
}

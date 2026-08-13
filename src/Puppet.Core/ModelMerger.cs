namespace Puppet.Core;

/// <summary>
/// Unions a freshly-built model into a previously saved one by element id.
/// Absence is not evidence of non-existence: elements only present in the
/// old file are always kept.
/// </summary>
public static class ModelMerger
{
    public static ModelDocument Merge(ModelDocument existing, ModelDocument fresh)
    {
        var byId = existing.Elements.ToDictionary(e => e.Id);

        foreach (var element in fresh.Elements)
        {
            byId[element.Id] = byId.TryGetValue(element.Id, out var existingElement)
                ? existingElement with
                {
                    Name = element.Name,
                    IsEnabled = element.IsEnabled,
                    Constraints = element.Constraints,
                }
                : element;
        }

        var mergedElements = byId.Values.ToList();

        // UnselectedTabPage is fully derivable from the merged structure, so
        // recompute it over everything. CollapsedNode/UnopenedMenu depend on
        // live ExpandCollapseState, which isn't persisted, so only this
        // walk's live findings can contribute those two reasons.
        var structural = CoverageDetector.DetectUnselectedTabPages(mergedElements);
        var liveFromFresh = fresh.Coverage.Unexplored
            .Where(u => u.Reason is "CollapsedNode" or "UnopenedMenu")
            .ToList();

        return new ModelDocument
        {
            AppId = fresh.AppId,
            AppTitle = fresh.AppTitle,
            ProcessName = fresh.ProcessName,
            BuiltAt = fresh.BuiltAt,
            Elements = mergedElements,
            Coverage = new CoverageReport
            {
                ElementCount = mergedElements.Count,
                Unexplored = [.. structural, .. liveFromFresh],
            },
        };
    }
}

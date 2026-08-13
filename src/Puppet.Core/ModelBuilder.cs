using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;

namespace Puppet.Core;

/// <summary>
/// Produces model.json from a live UIA tree (MB-1 through MB-10).
/// Property and pattern reads are batched with a CacheRequest (MB-3).
/// </summary>
public static class ModelBuilder
{
    // MB-5: only these patterns are recorded in the model.
    private static readonly (string Name, Func<AutomationBase, PatternId> Id, Func<AutomationElement, bool> IsSupported)[] PatternChecks =
    [
        ("Invoke", a => a.PatternLibrary.InvokePattern, e => e.Patterns.Invoke.IsSupported),
        ("Value", a => a.PatternLibrary.ValuePattern, e => e.Patterns.Value.IsSupported),
        ("Toggle", a => a.PatternLibrary.TogglePattern, e => e.Patterns.Toggle.IsSupported),
        ("RangeValue", a => a.PatternLibrary.RangeValuePattern, e => e.Patterns.RangeValue.IsSupported),
        ("SelectionItem", a => a.PatternLibrary.SelectionItemPattern, e => e.Patterns.SelectionItem.IsSupported),
        ("ExpandCollapse", a => a.PatternLibrary.ExpandCollapsePattern, e => e.Patterns.ExpandCollapse.IsSupported),
        ("LegacyIAccessible", a => a.PatternLibrary.LegacyIAccessiblePattern, e => e.Patterns.LegacyIAccessible.IsSupported),
    ];

    public static CacheRequest BuildCacheRequest(AutomationBase automation)
    {
        var request = new CacheRequest
        {
            TreeScope = TreeScope.Subtree,
            AutomationElementMode = AutomationElementMode.Full,
        };

        request.Add(automation.PropertyLibrary.Element.AutomationId);
        request.Add(automation.PropertyLibrary.Element.Name);
        request.Add(automation.PropertyLibrary.Element.ControlType);
        request.Add(automation.PropertyLibrary.Element.IsEnabled);
        request.Add(automation.PropertyLibrary.Element.NativeWindowHandle);

        foreach (var check in PatternChecks)
        {
            request.Add(check.Id(automation));
        }

        // Pattern-owned properties needed for constraints / defaultAction / coverage.
        request.Add(automation.PropertyLibrary.RangeValue.Minimum);
        request.Add(automation.PropertyLibrary.RangeValue.Maximum);
        request.Add(automation.PropertyLibrary.RangeValue.SmallChange);
        request.Add(automation.PropertyLibrary.ExpandCollapse.ExpandCollapseState);
        request.Add(automation.PropertyLibrary.LegacyIAccessible.DefaultAction);

        return request;
    }

    /// <summary>
    /// Walks <paramref name="mainWindow"/> and its cached descendants into
    /// a ModelDocument. Call inside an active CacheRequest scope obtained
    /// from <see cref="BuildCacheRequest"/>, with mainWindow itself having
    /// been fetched while that scope was active.
    /// </summary>
    public static ModelDocument Build(AutomationElement mainWindow, string processName)
    {
        var elements = new List<ModelElement>();
        var liveUnexplored = new List<UnexploredContainer>();

        var rootType = GetControlTypeName(mainWindow);
        Walk(mainWindow, [rootType], elements, liveUnexplored);

        var structuralUnexplored = CoverageDetector.DetectUnselectedTabPages(elements);

        return new ModelDocument
        {
            AppId = processName.ToLowerInvariant(),
            AppTitle = NullIfEmpty(mainWindow.Properties.Name.ValueOrDefault) ?? processName,
            ProcessName = processName,
            BuiltAt = DateTime.UtcNow,
            Elements = elements,
            Coverage = new CoverageReport
            {
                ElementCount = elements.Count,
                Unexplored = [.. structuralUnexplored, .. liveUnexplored],
            },
        };
    }

    private static void Walk(AutomationElement element, List<string> path, List<ModelElement> elements, List<UnexploredContainer> liveUnexplored)
    {
        var automationId = NullIfEmpty(element.Properties.AutomationId.ValueOrDefault);
        var controlType = GetControlTypeName(element);
        var id = ElementIdHasher.ComputeId(automationId, controlType, path);
        var patterns = PatternChecks.Where(p => p.IsSupported(element)).Select(p => p.Name).ToList();
        var nativeHandle = element.Properties.NativeWindowHandle.ValueOrDefault.ToInt64();
        var (mechanism, confidence) = MechanismResolver.Resolve(patterns, nativeHandle);

        elements.Add(new ModelElement
        {
            Id = id,
            AutomationId = automationId,
            Name = NullIfEmpty(element.Properties.Name.ValueOrDefault),
            ControlType = controlType,
            Path = path,
            NativeHandle = nativeHandle,
            Patterns = patterns,
            DefaultAction = ComputeDefaultAction(element, patterns),
            Mechanism = mechanism,
            Confidence = confidence,
            Constraints = ComputeConstraints(element, patterns),
            IsEnabled = element.Properties.IsEnabled.ValueOrDefault,
        });

        DetectLiveCoverage(element, controlType, id, liveUnexplored);

        foreach (var (child, segment) in BuildChildSegments(element.CachedChildren))
        {
            Walk(child, [.. path, segment], elements, liveUnexplored);
        }
    }

    private static IEnumerable<(AutomationElement Child, string Segment)> BuildChildSegments(AutomationElement[] children)
    {
        var typeNames = children.Select(GetControlTypeName).ToArray();
        var counts = typeNames.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
        var running = new Dictionary<string, int>();

        for (var i = 0; i < children.Length; i++)
        {
            var type = typeNames[i];
            string segment;
            if (counts[type] > 1)
            {
                running[type] = running.GetValueOrDefault(type) + 1;
                segment = $"{type}[{running[type]}]";
            }
            else
            {
                segment = type;
            }

            yield return (children[i], segment);
        }
    }

    private static string GetControlTypeName(AutomationElement element) =>
        element.Properties.ControlType.ValueOrDefault.ToString();

    private static string? ComputeDefaultAction(AutomationElement element, List<string> patterns)
    {
        var hasRicherPattern = patterns.Any(p => p != "LegacyIAccessible");
        if (hasRicherPattern || !patterns.Contains("LegacyIAccessible"))
        {
            return null;
        }

        return NullIfEmpty(element.Patterns.LegacyIAccessible.PatternOrDefault?.DefaultAction.ValueOrDefault);
    }

    private static ElementConstraints? ComputeConstraints(AutomationElement element, List<string> patterns)
    {
        if (!patterns.Contains("RangeValue"))
        {
            return null;
        }

        var rangeValue = element.Patterns.RangeValue.PatternOrDefault;
        if (rangeValue == null)
        {
            return null;
        }

        return new ElementConstraints
        {
            Minimum = rangeValue.Minimum.ValueOrDefault,
            Maximum = rangeValue.Maximum.ValueOrDefault,
            Step = rangeValue.SmallChange.ValueOrDefault,
        };
    }

    private static void DetectLiveCoverage(AutomationElement element, string controlType, string id, List<UnexploredContainer> liveUnexplored)
    {
        if (controlType is not ("TreeItem" or "MenuItem"))
        {
            return;
        }

        var expandCollapse = element.Patterns.ExpandCollapse.PatternOrDefault;
        if (expandCollapse == null || expandCollapse.ExpandCollapseState.ValueOrDefault != ExpandCollapseState.Collapsed)
        {
            return;
        }

        liveUnexplored.Add(new UnexploredContainer
        {
            ContainerId = id,
            ContainerType = controlType,
            Reason = controlType == "TreeItem" ? "CollapsedNode" : "UnopenedMenu",
        });
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;
}

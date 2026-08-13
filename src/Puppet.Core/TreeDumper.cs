using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;

namespace Puppet.Core;

/// <summary>
/// Walks a UI Automation subtree into an <see cref="ElementNode"/> tree.
/// Property and pattern-availability reads are batched with a
/// <see cref="CacheRequest"/> so the whole subtree comes back in as few
/// cross-process round trips as possible.
/// </summary>
public static class TreeDumper
{
    private static readonly (string Name, Func<AutomationBase, PatternId> Id, Func<AutomationElement, bool> IsSupported)[] PatternChecks =
    [
        ("Invoke", a => a.PatternLibrary.InvokePattern, e => e.Patterns.Invoke.IsSupported),
        ("Value", a => a.PatternLibrary.ValuePattern, e => e.Patterns.Value.IsSupported),
        ("RangeValue", a => a.PatternLibrary.RangeValuePattern, e => e.Patterns.RangeValue.IsSupported),
        ("Toggle", a => a.PatternLibrary.TogglePattern, e => e.Patterns.Toggle.IsSupported),
        ("SelectionItem", a => a.PatternLibrary.SelectionItemPattern, e => e.Patterns.SelectionItem.IsSupported),
        ("Selection", a => a.PatternLibrary.SelectionPattern, e => e.Patterns.Selection.IsSupported),
        ("ExpandCollapse", a => a.PatternLibrary.ExpandCollapsePattern, e => e.Patterns.ExpandCollapse.IsSupported),
        ("Scroll", a => a.PatternLibrary.ScrollPattern, e => e.Patterns.Scroll.IsSupported),
        ("ScrollItem", a => a.PatternLibrary.ScrollItemPattern, e => e.Patterns.ScrollItem.IsSupported),
        ("Grid", a => a.PatternLibrary.GridPattern, e => e.Patterns.Grid.IsSupported),
        ("GridItem", a => a.PatternLibrary.GridItemPattern, e => e.Patterns.GridItem.IsSupported),
        ("Table", a => a.PatternLibrary.TablePattern, e => e.Patterns.Table.IsSupported),
        ("TableItem", a => a.PatternLibrary.TableItemPattern, e => e.Patterns.TableItem.IsSupported),
        ("Text", a => a.PatternLibrary.TextPattern, e => e.Patterns.Text.IsSupported),
        ("Window", a => a.PatternLibrary.WindowPattern, e => e.Patterns.Window.IsSupported),
        ("Transform", a => a.PatternLibrary.TransformPattern, e => e.Patterns.Transform.IsSupported),
        ("MultipleView", a => a.PatternLibrary.MultipleViewPattern, e => e.Patterns.MultipleView.IsSupported),
        ("Dock", a => a.PatternLibrary.DockPattern, e => e.Patterns.Dock.IsSupported),
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
        request.Add(automation.PropertyLibrary.Element.ClassName);
        request.Add(automation.PropertyLibrary.Element.IsEnabled);
        request.Add(automation.PropertyLibrary.Element.IsOffscreen);
        request.Add(automation.PropertyLibrary.Element.NativeWindowHandle);

        foreach (var check in PatternChecks)
        {
            request.Add(check.Id(automation));
        }

        return request;
    }

    /// <summary>
    /// Builds an <see cref="ElementNode"/> tree from <paramref name="root"/>
    /// and its cached descendants. Call this inside an active
    /// <see cref="CacheRequest"/> scope obtained from
    /// <see cref="BuildCacheRequest"/>, with <paramref name="root"/> itself
    /// having been fetched while that scope was active.
    /// </summary>
    public static ElementNode Dump(AutomationElement root)
    {
        var node = new ElementNode
        {
            AutomationId = NullIfEmpty(root.Properties.AutomationId.ValueOrDefault),
            Name = NullIfEmpty(root.Properties.Name.ValueOrDefault),
            ControlType = root.Properties.ControlType.ValueOrDefault.ToString(),
            ClassName = NullIfEmpty(root.Properties.ClassName.ValueOrDefault),
            IsEnabled = root.Properties.IsEnabled.ValueOrDefault,
            IsOffscreen = root.Properties.IsOffscreen.ValueOrDefault,
            NativeWindowHandle = root.Properties.NativeWindowHandle.ValueOrDefault.ToInt64(),
            Patterns = PatternChecks.Where(p => p.IsSupported(root)).Select(p => p.Name).ToList(),
        };

        foreach (var child in root.CachedChildren)
        {
            node.Children.Add(Dump(child));
        }

        return node;
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;
}

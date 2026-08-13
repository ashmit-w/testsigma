namespace Puppet.Core;

/// <summary>
/// One node of a dumped UI Automation tree.
/// </summary>
public sealed class ElementNode
{
    public string? AutomationId { get; init; }
    public string? Name { get; init; }
    public required string ControlType { get; init; }
    public string? ClassName { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsOffscreen { get; init; }
    public long NativeWindowHandle { get; init; }
    public List<string> Patterns { get; init; } = [];
    public List<ElementNode> Children { get; init; } = [];
}

namespace Puppet.Core;

/// <summary>
/// One element of model.json, per docs/spec.md section 5.
/// </summary>
public sealed record ModelElement
{
    public required string Id { get; init; }
    public string? AutomationId { get; init; }
    public string? Name { get; init; }
    public required string ControlType { get; init; }
    public required List<string> Path { get; init; }
    public long NativeHandle { get; init; }
    public List<string> Patterns { get; init; } = [];
    public string? DefaultAction { get; init; }
    public string? Mechanism { get; init; }
    public int? Confidence { get; init; }
    public ElementConstraints? Constraints { get; init; }
    public bool IsEnabled { get; init; }
}

namespace Puppet.Core;

/// <summary>
/// model.json, per docs/spec.md section 5. Frozen shape - do not add
/// fields without updating the spec first.
/// </summary>
public sealed record ModelDocument
{
    public int SchemaVersion { get; init; } = 1;
    public required string AppId { get; init; }
    public required string AppTitle { get; init; }
    public required string ProcessName { get; init; }
    public DateTime BuiltAt { get; init; }
    public List<ModelElement> Elements { get; init; } = [];
    public CoverageReport Coverage { get; init; } = new();
}

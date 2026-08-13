namespace Puppet.Core;

/// <summary>
/// One step of a Flow. The target is resolved against the live tree at
/// the moment this step executes - never through a model lookup, since a
/// checkpoint replay has no single model covering both the launch-state
/// controls and whatever deep state a later step navigates to. The block
/// that produced this step therefore carries its own locator: AutomationId
/// is tried first, falling back to TargetPath (the same segments
/// ModelBuilder records on ModelElement.Path) when it's absent or fails
/// to resolve.
/// </summary>
public sealed record FlowStep
{
    public required string Description { get; init; }
    public string? AutomationId { get; init; }
    public required List<string> TargetPath { get; init; }
    public required ActionArgs Action { get; init; }
}

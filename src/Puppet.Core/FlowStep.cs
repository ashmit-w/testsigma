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
///
/// A step is either an action (Action set) or an assertion (Assert set) -
/// never both. Assertions read state rather than acting (AS-1..AS-4).
/// </summary>
public sealed record FlowStep
{
    public required string Description { get; init; }
    public string? AutomationId { get; init; }
    public required List<string> TargetPath { get; init; }
    public ActionArgs? Action { get; init; }
    public AssertSpec? Assert { get; init; }
}

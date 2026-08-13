namespace Puppet.Core;

/// <summary>
/// One step of a Flow. The target is identified by its structural path
/// (the same segments ModelBuilder records on ModelElement.Path) rather
/// than by id, since a step must still be resolvable against a fresh
/// scan taken after replay has re-launched the process.
/// </summary>
public sealed record FlowStep
{
    public required string Description { get; init; }
    public required List<string> TargetPath { get; init; }
    public required ActionArgs Action { get; init; }
}

namespace Puppet.Core;

/// <summary>
/// A container detected during the walk whose children are not fully
/// realized in the live UI Automation tree.
/// </summary>
public sealed record UnexploredContainer
{
    public required string ContainerId { get; init; }
    public required string ContainerType { get; init; }
    public required string Reason { get; init; }
}

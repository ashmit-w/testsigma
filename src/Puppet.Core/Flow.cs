namespace Puppet.Core;

/// <summary>An ordered list of blocks to replay from a fresh process launch.</summary>
public sealed record Flow
{
    public List<FlowStep> Steps { get; init; } = [];
}

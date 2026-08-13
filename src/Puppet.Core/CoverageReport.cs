namespace Puppet.Core;

public sealed record CoverageReport
{
    public int ElementCount { get; init; }
    public List<UnexploredContainer> Unexplored { get; init; } = [];
}

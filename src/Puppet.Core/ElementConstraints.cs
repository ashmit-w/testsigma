namespace Puppet.Core;

/// <summary>
/// Minimum, maximum, and step extracted from a RangeValue pattern.
/// </summary>
public sealed record ElementConstraints
{
    public required double Minimum { get; init; }
    public required double Maximum { get; init; }
    public required double Step { get; init; }
}

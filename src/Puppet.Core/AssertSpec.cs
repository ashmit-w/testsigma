namespace Puppet.Core;

/// <summary>An assertion step's check: what kind, and (for text assertions) what value is expected.</summary>
public sealed record AssertSpec
{
    public required AssertKind Kind { get; init; }
    public string? ExpectedText { get; init; }
}

namespace Puppet.Core;

public sealed record InteractionResult
{
    public required string Mechanism { get; init; }
    public required int Confidence { get; init; }
    public required bool Success { get; init; }
    public FailureCause? FailureCause { get; init; }
    public string? Value { get; init; }
}

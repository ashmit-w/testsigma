namespace Puppet.Core;

public enum StepStatus
{
    Passed,
    Failed,
    Skipped,
}

/// <summary>Per-step outcome of a replay (TR-6).</summary>
public sealed record StepResult
{
    public required string Description { get; init; }
    public required StepStatus Status { get; init; }
    public TimeSpan Duration { get; init; }
    public string? Mechanism { get; init; }
    public int? Confidence { get; init; }
    public FailureCause? FailureCause { get; init; }
    public string? Message { get; init; }
}

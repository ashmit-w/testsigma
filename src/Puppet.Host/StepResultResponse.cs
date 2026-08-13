using Puppet.Core;

namespace Puppet.Host;

/// <summary>
/// Puppet.Core.StepResult reshaped for the wire: DurationMs instead of a
/// TimeSpan, since that's what the spec for /session/run asks for.
/// </summary>
public sealed record StepResultResponse
{
    public required string Description { get; init; }
    public required StepStatus Status { get; init; }
    public double DurationMs { get; init; }
    public string? Mechanism { get; init; }
    public int? Confidence { get; init; }
    public FailureCause? FailureCause { get; init; }
    public string? Message { get; init; }

    public static StepResultResponse From(StepResult result) => new()
    {
        Description = result.Description,
        Status = result.Status,
        DurationMs = result.Duration.TotalMilliseconds,
        Mechanism = result.Mechanism,
        Confidence = result.Confidence,
        FailureCause = result.FailureCause,
        Message = result.Message,
    };
}

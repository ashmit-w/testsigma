namespace Puppet.Core;

/// <summary>Result of AppSession.Replay: one entry per step, plus the model scanned after the last executed step.</summary>
public sealed record ReplayResult
{
    public required List<StepResult> StepResults { get; init; }
    public required ModelDocument Model { get; init; }
}

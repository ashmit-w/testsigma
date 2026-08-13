namespace Puppet.Host;

public sealed record StepArgsRequest
{
    public string? Text { get; init; }
    public bool? TargetState { get; init; }
    public int? Index { get; init; }
}

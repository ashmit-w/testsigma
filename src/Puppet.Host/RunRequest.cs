namespace Puppet.Host;

public sealed record RunRequest
{
    public required List<StepRequest> Steps { get; init; }
}

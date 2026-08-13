namespace Puppet.Host;

public sealed record StartRequest
{
    public required string ExePath { get; init; }
}

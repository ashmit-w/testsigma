namespace Puppet.Core;

public sealed record ActionArgs
{
    public required ActionKind Kind { get; init; }
    public string? Text { get; init; }
    public bool? TargetState { get; init; }
    public int? Index { get; init; }
}

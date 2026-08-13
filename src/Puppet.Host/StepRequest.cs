namespace Puppet.Host;

/// <summary>
/// One step exactly as the editor sends it: a complete locator plus an
/// action. No elementId, no model lookup - FlowStepMapper maps this
/// straight onto Puppet.Core.FlowStep.
/// </summary>
public sealed record StepRequest
{
    public required string Description { get; init; }
    public string? AutomationId { get; init; }
    public required string ControlType { get; init; }
    public required List<string> Path { get; init; }
    public required string Action { get; init; }
    public StepArgsRequest? Args { get; init; }
}

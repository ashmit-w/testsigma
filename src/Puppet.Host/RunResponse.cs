using Puppet.Core;

namespace Puppet.Host;

public sealed record RunResponse
{
    public required List<StepResultResponse> StepResults { get; init; }
    public required PaletteDocument Palette { get; init; }
    public required CoverageReport Coverage { get; init; }
    public int? ProcessId { get; init; }
}

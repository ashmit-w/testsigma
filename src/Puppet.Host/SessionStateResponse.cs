using Puppet.Core;

namespace Puppet.Host;

/// <summary>Response for /session/start, /session/reset, and /session/state.</summary>
public sealed record SessionStateResponse
{
    public required PaletteDocument Palette { get; init; }
    public required CoverageReport Coverage { get; init; }
    public int? ProcessId { get; init; }
}

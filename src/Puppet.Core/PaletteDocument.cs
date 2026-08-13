namespace Puppet.Core;

/// <summary>
/// palette.json: a Blockly block/toolbox definition set, generated purely
/// from a ModelDocument (BG-1..BG-3). Not part of the frozen model.json
/// schema - this is BlockGenerator's own output shape.
/// </summary>
public sealed record PaletteDocument
{
    public required string AppId { get; init; }
    public required string AppTitle { get; init; }
    public DateTime ModelBuiltAt { get; init; }
    public required CoverageReport Coverage { get; init; }
    public List<PaletteBlock> Blocks { get; init; } = [];
    public required PaletteToolbox Toolbox { get; init; }
}

/// <summary>One Blockly JSON block definition, plus the metadata the runner needs to execute it.</summary>
public sealed record PaletteBlock
{
    public required string Type { get; init; }
    public required string Message0 { get; init; }
    public List<Dictionary<string, object?>> Args0 { get; init; } = [];
    public object? PreviousStatement { get; init; }
    public object? NextStatement { get; init; }
    public required int Colour { get; init; }
    public string? Tooltip { get; init; }
    public required PuppetBlockMeta Puppet { get; init; }
}

/// <summary>
/// Non-Blockly metadata: which element and action a block maps to, for
/// the Test Runner. AutomationId and Path make the block a self-contained
/// locator - replay resolves against the live tree at execution time, not
/// through a model lookup, so the block must carry everything needed to
/// find its element itself.
/// </summary>
public sealed record PuppetBlockMeta
{
    public required string ElementId { get; init; }
    public string? AutomationId { get; init; }
    public required string ControlType { get; init; }
    public required List<string> Path { get; init; }
    public required string Action { get; init; }
    public string? AssertKind { get; init; }
    public bool? TargetState { get; init; }
    public bool? Clear { get; init; }
    public string? Mechanism { get; init; }
    public int? Confidence { get; init; }
    public bool LowConfidence { get; init; }
}

public sealed record PaletteToolbox
{
    public string Kind { get; init; } = "flyoutToolbox";
    public List<PaletteToolboxEntry> Contents { get; init; } = [];
}

public sealed record PaletteToolboxEntry
{
    public string Kind { get; init; } = "block";
    public required string Type { get; init; }
}

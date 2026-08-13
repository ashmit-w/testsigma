namespace Puppet.Core;

/// <summary>
/// Produces palette.json from a ModelDocument (docs/spec.md section 6.3).
/// A pure function of the model: same ModelDocument in, same
/// PaletteDocument out, no running application required. Follows the
/// capability -&gt; block rule table exactly (BG-1) - there is no
/// application-specific branch anywhere in this file.
/// </summary>
public static class BlockGenerator
{
    private const int LowConfidenceColour = 0;

    public static PaletteDocument Generate(ModelDocument model)
    {
        var blocks = new List<PaletteBlock>();

        foreach (var element in model.Elements)
        {
            if (!element.IsEnabled)
            {
                continue;
            }

            AddTier3Blocks(element, blocks);
            AddLegacyBlocks(element, blocks);
        }

        return new PaletteDocument
        {
            AppId = model.AppId,
            AppTitle = model.AppTitle,
            ModelBuiltAt = model.BuiltAt,
            Coverage = model.Coverage,
            Blocks = blocks,
            Toolbox = new PaletteToolbox
            {
                Contents = [.. blocks.Select(b => new PaletteToolboxEntry { Type = b.Type })],
            },
        };
    }

    private static void AddTier3Blocks(ModelElement element, List<PaletteBlock> blocks)
    {
        if (element.Patterns.Contains("Invoke"))
        {
            blocks.Add(Build(element, "click", "click %1", TargetArgs(element), Colour.Invoke, "Invoke"));
        }

        if (element.Patterns.Contains("Value"))
        {
            blocks.Add(Build(element, "type", "type %1 into %2",
                [TextField("TEXT", ""), TargetArg(element)], Colour.Value, "SetValue"));

            blocks.Add(Build(element, "clear", "clear %1", TargetArgs(element), Colour.Value, "SetValue",
                clear: true));

            blocks.Add(Build(element, "expect_text", "expect %1 has text %2",
                [TargetArg(element), TextField("TEXT", "")], Colour.Value, "AssertTextEquals", assertKind: "TextEquals"));
        }

        if (element.Patterns.Contains("Toggle"))
        {
            blocks.Add(Build(element, "set_checked", "set %1 checked", TargetArgs(element), Colour.Toggle,
                "Toggle", targetState: true));

            blocks.Add(Build(element, "set_unchecked", "set %1 unchecked", TargetArgs(element), Colour.Toggle,
                "Toggle", targetState: false));

            blocks.Add(Build(element, "expect_checked", "expect %1 is checked", TargetArgs(element), Colour.Toggle,
                "AssertChecked", assertKind: "IsChecked"));
        }

        if (element.Patterns.Contains("RangeValue") && element.Constraints is { } constraints)
        {
            blocks.Add(Build(element, "set_range", "set %1 to %2",
                [TargetArg(element), NumberField("VALUE", constraints)], Colour.RangeValue, "SetRangeValue"));
        }

        if (element.Patterns.Contains("SelectionItem"))
        {
            blocks.Add(Build(element, "select", "select %1", TargetArgs(element), Colour.SelectionItem,
                "SelectIndex"));

            blocks.Add(Build(element, "expect_selected", "expect %1 is selected", TargetArgs(element),
                Colour.SelectionItem, "GetSelectionState", assertKind: "IsSelected"));
        }

        if (element.Patterns.Contains("ExpandCollapse"))
        {
            blocks.Add(Build(element, "expand", "expand %1", TargetArgs(element), Colour.ExpandCollapse, "Expand"));
            blocks.Add(Build(element, "collapse", "collapse %1", TargetArgs(element), Colour.ExpandCollapse,
                "Collapse"));
        }
    }

    private static void AddLegacyBlocks(ModelElement element, List<PaletteBlock> blocks)
    {
        if (!element.Patterns.Contains("LegacyIAccessible") || element.Patterns.Any(p => p != "LegacyIAccessible"))
        {
            return;
        }

        switch (element.DefaultAction)
        {
            case "Press":
                blocks.Add(Build(element, "activate", "activate %1", TargetArgs(element), Colour.Invoke, "Invoke"));
                break;
            case "Check":
                blocks.Add(Build(element, "toggle", "toggle %1", TargetArgs(element), Colour.Toggle, "Toggle"));
                break;
        }
    }

    private static PaletteBlock Build(
        ModelElement element,
        string verb,
        string message0,
        List<Dictionary<string, object?>> args0,
        int colour,
        string action,
        string? assertKind = null,
        bool? targetState = null,
        bool? clear = null)
    {
        var lowConfidence = element.Confidence is 1 or 2;

        return new PaletteBlock
        {
            Type = $"{verb}_{element.Id}",
            Message0 = lowConfidence ? $"⚠ {message0}" : message0,
            Args0 = args0,
            PreviousStatement = null,
            NextStatement = null,
            Colour = lowConfidence ? LowConfidenceColour : colour,
            Tooltip = lowConfidence
                ? $"Low confidence (tier {element.Confidence}) - resolved via {element.Mechanism}, may not be reliable."
                : null,
            Puppet = new PuppetBlockMeta
            {
                ElementId = element.Id,
                AutomationId = element.AutomationId,
                ControlType = element.ControlType,
                Path = element.Path,
                Action = action,
                AssertKind = assertKind,
                TargetState = targetState,
                Clear = clear,
                Mechanism = element.Mechanism,
                Confidence = element.Confidence,
                LowConfidence = lowConfidence,
            },
        };
    }

    private static List<Dictionary<string, object?>> TargetArgs(ModelElement element) => [TargetArg(element)];

    private static Dictionary<string, object?> TargetArg(ModelElement element) =>
        new()
        {
            ["type"] = "field_label_serializable",
            ["name"] = "TARGET",
            ["text"] = element.Name ?? element.AutomationId ?? element.ControlType,
        };

    private static Dictionary<string, object?> TextField(string name, string defaultValue) =>
        new()
        {
            ["type"] = "field_input",
            ["name"] = name,
            ["text"] = defaultValue,
        };

    private static Dictionary<string, object?> NumberField(string name, ElementConstraints constraints)
    {
        var field = new Dictionary<string, object?>
        {
            ["type"] = "field_number",
            ["name"] = name,
            ["value"] = constraints.Minimum,
            ["min"] = constraints.Minimum,
            ["max"] = constraints.Maximum,
        };

        if (constraints.Step > 0)
        {
            field["precision"] = constraints.Step;
        }

        return field;
    }

    private static class Colour
    {
        public const int Invoke = 210;
        public const int Value = 160;
        public const int Toggle = 290;
        public const int RangeValue = 65;
        public const int SelectionItem = 120;
        public const int ExpandCollapse = 230;
    }
}

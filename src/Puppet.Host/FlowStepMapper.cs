using Puppet.Core;

namespace Puppet.Host;

/// <summary>
/// Maps a StepRequest straight onto a FlowStep. No model, no lookup - the
/// request already carries a complete locator (automationId, path), so
/// this only has to decide whether Action is an interaction or an
/// assertion (see AssertKind).
/// </summary>
public static class FlowStepMapper
{
    private const string AssertPrefix = "Assert";

    public static FlowStep Map(StepRequest request)
    {
        var args = request.Args ?? new StepArgsRequest();

        if (request.Action.StartsWith(AssertPrefix, StringComparison.Ordinal)
            && Enum.TryParse<AssertKind>(request.Action[AssertPrefix.Length..], out var assertKind))
        {
            return new FlowStep
            {
                Description = request.Description,
                AutomationId = request.AutomationId,
                TargetPath = request.Path,
                Assert = new AssertSpec { Kind = assertKind, ExpectedText = args.Text },
            };
        }

        if (!Enum.TryParse<ActionKind>(request.Action, out var actionKind))
        {
            throw new ArgumentException(
                $"Unknown action '{request.Action}'. Expected one of: Invoke, Toggle, SetValue, GetValue, " +
                $"SelectIndex, or Assert followed by Exists, NotExists, TextEquals, TextContains, Enabled, " +
                $"Disabled, Checked, Unchecked.");
        }

        return new FlowStep
        {
            Description = request.Description,
            AutomationId = request.AutomationId,
            TargetPath = request.Path,
            Action = new ActionArgs
            {
                Kind = actionKind,
                Text = args.Text,
                TargetState = args.TargetState,
                Index = args.Index,
            },
        };
    }
}

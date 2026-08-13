using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace Puppet.Core;

/// <summary>
/// AS-1..AS-4: assertions read state rather than acting. Text assertions
/// go through the Interaction Layer's GetValue tier (same mechanism/
/// confidence reporting as any other read), since that's the one already
/// built to answer "what mechanism can read this element's value". Enabled
/// and Checked read the relevant UIA property/pattern directly - there is
/// no tiered fallback for observing state, only for acting on it.
///
/// Existence (Exists/NotExists) is handled by the caller, since it's
/// about whether resolution itself succeeded rather than about anything
/// read from a resolved element.
/// </summary>
public static class AssertionExecutor
{
    private const string PropertyMechanism = "UiaProperty";

    public static InteractionResult Execute(AutomationElement? element, AssertSpec spec, InteractionResolver interactionResolver)
    {
        if (element == null)
        {
            return Fail(PropertyMechanism, 0, FailureCause.NotFound);
        }

        return spec.Kind switch
        {
            AssertKind.TextEquals or AssertKind.TextContains => ExecuteText(element, spec, interactionResolver),
            AssertKind.Enabled or AssertKind.Disabled => ExecuteEnabled(element, spec),
            AssertKind.Checked or AssertKind.Unchecked => ExecuteChecked(element, spec),
            _ => Fail(PropertyMechanism, 0, FailureCause.NoMechanismSucceeded),
        };
    }

    private static InteractionResult ExecuteText(AutomationElement element, AssertSpec spec, InteractionResolver interactionResolver)
    {
        var read = interactionResolver.Execute(element, new ActionArgs { Kind = ActionKind.GetValue });
        if (!read.Success)
        {
            return read;
        }

        var actual = read.Value ?? string.Empty;
        var expected = spec.ExpectedText ?? string.Empty;
        var matched = spec.Kind == AssertKind.TextEquals
            ? actual == expected
            : actual.Contains(expected, StringComparison.Ordinal);

        return matched ? read : read with { Success = false };
    }

    private static InteractionResult ExecuteEnabled(AutomationElement element, AssertSpec spec)
    {
        var isEnabled = element.Properties.IsEnabled.ValueOrDefault;
        var expected = spec.Kind == AssertKind.Enabled;
        return isEnabled == expected ? Pass(PropertyMechanism, 3) : Fail(PropertyMechanism, 3, null);
    }

    private static InteractionResult ExecuteChecked(AutomationElement element, AssertSpec spec)
    {
        if (!element.Patterns.Toggle.IsSupported)
        {
            return Fail(MechanismNames.UiaPattern, 0, FailureCause.NoMechanismSucceeded);
        }

        var isChecked = element.Patterns.Toggle.Pattern.ToggleState.ValueOrDefault == ToggleState.On;
        var expected = spec.Kind == AssertKind.Checked;
        return isChecked == expected ? Pass(MechanismNames.UiaPattern, 3) : Fail(MechanismNames.UiaPattern, 3, null);
    }

    private static InteractionResult Pass(string mechanism, int confidence) =>
        new() { Mechanism = mechanism, Confidence = confidence, Success = true };

    private static InteractionResult Fail(string mechanism, int confidence, FailureCause? cause) =>
        new() { Mechanism = mechanism, Confidence = confidence, Success = false, FailureCause = cause };
}

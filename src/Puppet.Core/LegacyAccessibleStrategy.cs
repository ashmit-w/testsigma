using FlaUI.Core.AutomationElements;

namespace Puppet.Core;

/// <summary>
/// Tier 2: the MSAA bridge, via DoDefaultAction() only - no parameters,
/// no return value, so it can satisfy "perform this element's one
/// default action" (Invoke/Toggle/SelectIndex) but never SetValue/GetValue,
/// which need to carry data in or out.
/// </summary>
public sealed class LegacyAccessibleStrategy : IInteractionStrategy
{
    public string Name => MechanismNames.LegacyIAccessible;
    public int Confidence => 2;

    public bool CanHandle(AutomationElement element, ActionKind actionKind) =>
        actionKind is ActionKind.Invoke or ActionKind.Toggle or ActionKind.SelectIndex
        && element.Patterns.LegacyIAccessible.IsSupported;

    public InteractionResult Execute(AutomationElement element, ActionArgs args)
    {
        var pattern = element.Patterns.LegacyIAccessible.PatternOrDefault;
        if (pattern == null)
        {
            return Failure();
        }

        try
        {
            pattern.DoDefaultAction();
            return Success();
        }
        catch
        {
            return Failure();
        }
    }

    private InteractionResult Success() =>
        new() { Mechanism = Name, Confidence = Confidence, Success = true };

    private InteractionResult Failure() =>
        new() { Mechanism = Name, Confidence = Confidence, Success = false };
}

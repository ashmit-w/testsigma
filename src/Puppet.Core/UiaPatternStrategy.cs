using FlaUI.Core.AutomationElements;

namespace Puppet.Core;

/// <summary>
/// Tier 3: UIA control patterns. Highest confidence - covers WPF fully
/// and most Win32 common controls via the built-in HWND provider.
/// </summary>
public sealed class UiaPatternStrategy : IInteractionStrategy
{
    public string Name => MechanismNames.UiaPattern;
    public int Confidence => 3;

    public bool CanHandle(AutomationElement element, ActionKind actionKind) => actionKind switch
    {
        ActionKind.Invoke => element.Patterns.Invoke.IsSupported,
        ActionKind.Toggle => element.Patterns.Toggle.IsSupported,
        ActionKind.SetValue => element.Patterns.Value.IsSupported,
        ActionKind.GetValue => element.Patterns.Value.IsSupported,
        ActionKind.SelectIndex => element.Patterns.SelectionItem.IsSupported,
        _ => false,
    };

    public InteractionResult Execute(AutomationElement element, ActionArgs args)
    {
        try
        {
            switch (args.Kind)
            {
                case ActionKind.Invoke:
                    element.Patterns.Invoke.Pattern.Invoke();
                    return Success();

                case ActionKind.Toggle:
                    element.Patterns.Toggle.Pattern.Toggle();
                    return Success();

                case ActionKind.SetValue:
                    element.Patterns.Value.Pattern.SetValue(args.Text ?? string.Empty);
                    return Success();

                case ActionKind.GetValue:
                    return Success(element.Patterns.Value.Pattern.Value.ValueOrDefault);

                case ActionKind.SelectIndex:
                    element.Patterns.SelectionItem.Pattern.Select();
                    return Success();

                default:
                    return Failure();
            }
        }
        catch
        {
            return Failure();
        }
    }

    private InteractionResult Success(string? value = null) =>
        new() { Mechanism = Name, Confidence = Confidence, Success = true, Value = value };

    private InteractionResult Failure() =>
        new() { Mechanism = Name, Confidence = Confidence, Success = false };
}

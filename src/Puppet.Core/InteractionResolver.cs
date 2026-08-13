using FlaUI.Core.AutomationElements;

namespace Puppet.Core;

/// <summary>
/// IL-2: resolves strategies in descending confidence order, first
/// success wins. IL-5: distinguishes not-found / disabled / no mechanism
/// succeeded. Element resolution (locating the element in the first
/// place) is not this layer's job - callers pass the outcome of that,
/// which may be null.
/// </summary>
public sealed class InteractionResolver
{
    private readonly IReadOnlyList<IInteractionStrategy> strategies;

    public InteractionResolver(IEnumerable<IInteractionStrategy>? strategies = null)
    {
        this.strategies = (strategies ?? DefaultStrategies())
            .OrderByDescending(s => s.Confidence)
            .ToList();
    }

    public static IEnumerable<IInteractionStrategy> DefaultStrategies() =>
    [
        new UiaPatternStrategy(),
        new LegacyAccessibleStrategy(),
        new Win32MessageStrategy(),
    ];

    public InteractionResult Execute(AutomationElement? element, ActionArgs args)
    {
        if (element == null)
        {
            return NotResolved(FailureCause.NotFound);
        }

        if (!element.Properties.IsEnabled.ValueOrDefault)
        {
            return NotResolved(FailureCause.FoundButDisabled);
        }

        foreach (var strategy in strategies)
        {
            if (!strategy.CanHandle(element, args.Kind))
            {
                continue;
            }

            var result = strategy.Execute(element, args);
            if (result.Success)
            {
                return result;
            }
        }

        return NotResolved(FailureCause.NoMechanismSucceeded);
    }

    private static InteractionResult NotResolved(FailureCause cause) => new()
    {
        Mechanism = "None",
        Confidence = 0,
        Success = false,
        FailureCause = cause,
    };
}

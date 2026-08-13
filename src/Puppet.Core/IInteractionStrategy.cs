using FlaUI.Core.AutomationElements;

namespace Puppet.Core;

/// <summary>
/// One interaction tier (docs/spec.md section 6.2, IL-1). Implementations
/// never let an exception escape Execute - a failed attempt is reported
/// as InteractionResult.Success == false, not a thrown exception, so the
/// resolver can cleanly fall through to the next tier.
/// </summary>
public interface IInteractionStrategy
{
    string Name { get; }
    int Confidence { get; }
    bool CanHandle(AutomationElement element, ActionKind actionKind);
    InteractionResult Execute(AutomationElement element, ActionArgs args);
}

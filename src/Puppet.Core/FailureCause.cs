namespace Puppet.Core;

/// <summary>
/// The three failure causes IL-5 requires reporting be distinguished.
/// </summary>
public enum FailureCause
{
    NotFound,
    FoundButDisabled,
    NoMechanismSucceeded,
}

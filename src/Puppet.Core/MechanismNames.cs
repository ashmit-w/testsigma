namespace Puppet.Core;

/// <summary>
/// Shared mechanism name constants, so a live strategy's <see cref="IInteractionStrategy.Name"/>
/// and the static <see cref="MechanismResolver"/> can never drift apart.
/// </summary>
public static class MechanismNames
{
    public const string UiaPattern = "UiaPattern";
    public const string LegacyIAccessible = "LegacyIAccessible";
    public const string Win32Message = "Win32Message";
}

namespace Puppet.Core;

/// <summary>
/// MB-8: static, build-time resolution of which interaction tier would
/// apply to an element, per docs/spec.md section 6.2's tier table. This
/// never invokes anything - it only inspects already-known patterns and
/// the native handle, matching "no fallback mechanism may participate in
/// model building."
/// </summary>
public static class MechanismResolver
{
    private static readonly HashSet<string> Tier3Patterns =
        ["Invoke", "Value", "Toggle", "RangeValue", "SelectionItem", "ExpandCollapse"];

    public static (string? Mechanism, int? Confidence) Resolve(IReadOnlyList<string> patterns, long nativeHandle)
    {
        if (patterns.Any(Tier3Patterns.Contains))
        {
            return (MechanismNames.UiaPattern, 3);
        }

        if (patterns.Contains("LegacyIAccessible"))
        {
            return (MechanismNames.LegacyIAccessible, 2);
        }

        if (nativeHandle != 0)
        {
            return (MechanismNames.Win32Message, 1);
        }

        return (null, null);
    }
}

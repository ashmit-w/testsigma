namespace Puppet.Core;

/// <summary>
/// One entry per Win32 message Tier 1 is allowed to send (docs/spec.md
/// section 6.2), each with a UIA and/or LegacyIAccessible equivalent.
/// </summary>
public enum ActionKind
{
    Invoke,
    Toggle,
    SetValue,
    GetValue,
    SelectIndex,
}

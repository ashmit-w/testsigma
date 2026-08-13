namespace Puppet.Core;

/// <summary>AS-1..AS-4 (docs/spec.md section 6.6): what an assertion step checks.</summary>
public enum AssertKind
{
    Exists,
    NotExists,
    TextEquals,
    TextContains,
    Enabled,
    Disabled,
    Checked,
    Unchecked,
}

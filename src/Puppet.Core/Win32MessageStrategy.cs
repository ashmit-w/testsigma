using System.Text;
using FlaUI.Core.AutomationElements;

namespace Puppet.Core;

/// <summary>
/// Tier 1: raw Win32 window messages against the captured HWND. Only
/// tried when tiers 2 and 3 both fail.
/// </summary>
public sealed class Win32MessageStrategy : IInteractionStrategy
{
    public string Name => MechanismNames.Win32Message;
    public int Confidence => 1;

    public bool CanHandle(AutomationElement element, ActionKind actionKind) =>
        GetHandle(element) != IntPtr.Zero
        && actionKind is ActionKind.Invoke or ActionKind.Toggle or ActionKind.SetValue
            or ActionKind.GetValue or ActionKind.SelectIndex;

    public InteractionResult Execute(AutomationElement element, ActionArgs args)
    {
        var handle = GetHandle(element);
        if (handle == IntPtr.Zero)
        {
            return Failure();
        }

        try
        {
            switch (args.Kind)
            {
                case ActionKind.Invoke:
                    NativeMethods.SendMessage(handle, NativeMethods.BmClick, IntPtr.Zero, IntPtr.Zero);
                    return Success();

                case ActionKind.Toggle:
                    // BM_GETCHECK isn't in the allowed message set, so Tier 1
                    // cannot read-before-write. Refuse rather than guess.
                    if (args.TargetState is not { } targetState)
                    {
                        return Failure();
                    }

                    NativeMethods.SendMessage(handle, NativeMethods.BmSetCheck, new IntPtr(targetState ? 1 : 0), IntPtr.Zero);
                    return Success();

                case ActionKind.SetValue:
                    NativeMethods.SendMessageText(handle, NativeMethods.WmSetText, IntPtr.Zero, args.Text ?? string.Empty);
                    return Success();

                case ActionKind.GetValue:
                    var buffer = new StringBuilder(1024);
                    NativeMethods.SendMessageGetText(handle, NativeMethods.WmGetText, new IntPtr(buffer.Capacity), buffer);
                    return Success(buffer.ToString());

                case ActionKind.SelectIndex:
                    if (args.Index is not { } index)
                    {
                        return Failure();
                    }

                    NativeMethods.SendMessage(handle, NativeMethods.CbSetCurSel, new IntPtr(index), IntPtr.Zero);
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

    private static IntPtr GetHandle(AutomationElement element) =>
        element.Properties.NativeWindowHandle.ValueOrDefault;

    private InteractionResult Success(string? value = null) =>
        new() { Mechanism = Name, Confidence = Confidence, Success = true, Value = value };

    private InteractionResult Failure() =>
        new() { Mechanism = Name, Confidence = Confidence, Success = false };
}

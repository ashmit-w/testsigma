using System.Runtime.InteropServices;
using System.Text;

namespace Puppet.Core;

/// <summary>
/// The Tier 1 message set (docs/spec.md section 6.2) - deliberately just
/// these five. WM_SETTEXT/WM_GETTEXT are safe cross-process because
/// user32 has always thunked their string payload itself; none of these
/// require a struct manually marshalled into the target's address space.
/// </summary>
internal static class NativeMethods
{
    internal const int BmClick = 0x00F5;
    internal const int BmSetCheck = 0x00F1;
    internal const int WmSetText = 0x000C;
    internal const int WmGetText = 0x000D;
    internal const int CbSetCurSel = 0x014E;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
    internal static extern IntPtr SendMessageText(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
    internal static extern IntPtr SendMessageGetText(IntPtr hWnd, int msg, IntPtr wParam, StringBuilder lParam);
}

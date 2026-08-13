using System.Diagnostics;
using FlaUI.Core;

namespace Puppet.Core;

/// <summary>
/// Attaches to a running target process by name.
/// </summary>
public static class ProcessAttacher
{
    public static Application Attach(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        if (processes.Length == 0)
        {
            throw new InvalidOperationException($"No running process named '{processName}' was found.");
        }

        if (processes.Length > 1)
        {
            throw new InvalidOperationException(
                $"Multiple running processes named '{processName}' were found ({processes.Length}). Attach by PID is not yet supported.");
        }

        return Application.Attach(processes[0]);
    }
}

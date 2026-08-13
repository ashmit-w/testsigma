using System.Diagnostics;
using FlaUI.Core;

namespace Puppet.Core;

/// <summary>
/// Attaches to a running target process by name or PID (MB-1).
/// </summary>
public static class ProcessAttacher
{
    public static Application AttachByName(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        if (processes.Length == 0)
        {
            throw new InvalidOperationException($"No running process named '{processName}' was found.");
        }

        if (processes.Length > 1)
        {
            throw new InvalidOperationException(
                $"Multiple running processes named '{processName}' were found ({processes.Length}). Attach by PID instead.");
        }

        return Application.Attach(processes[0]);
    }

    public static Application AttachByPid(int pid)
    {
        try
        {
            return Application.Attach(pid);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"No running process with PID {pid} was found.", ex);
        }
    }
}

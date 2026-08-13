using FlaUI.UIA3;

namespace Puppet.Core;

/// <summary>
/// Facade over the dedicated UIA STA thread. All FlaUI/UIA3 objects are
/// created and used only from inside <see cref="UiaThread"/> callbacks,
/// since they wrap apartment-affine COM pointers.
/// </summary>
public sealed class AutomationSession : IDisposable
{
    private readonly UiaThread thread = new();
    private UIA3Automation? automation;

    public Task<ElementNode> DumpTreeAsync(string processName) =>
        thread.InvokeAsync(() => DumpTree(processName));

    private ElementNode DumpTree(string processName)
    {
        var automation = GetOrCreateAutomation();
        var app = ProcessAttacher.Attach(processName);

        using (TreeDumper.BuildCacheRequest(automation).Activate())
        {
            var mainWindow = app.GetMainWindow(automation);
            return TreeDumper.Dump(mainWindow);
        }
    }

    private UIA3Automation GetOrCreateAutomation() => automation ??= new UIA3Automation();

    public void Dispose()
    {
        thread.InvokeAsync(() =>
        {
            automation?.Dispose();
            return true;
        }).GetAwaiter().GetResult();
        thread.Dispose();
    }
}

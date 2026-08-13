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

    public Task<ModelDocument> BuildModelAsync(string? processName, int? pid) =>
        thread.InvokeAsync(() => BuildModel(processName, pid));

    private ElementNode DumpTree(string processName)
    {
        var automation = GetOrCreateAutomation();
        var app = ProcessAttacher.AttachByName(processName);

        using (TreeDumper.BuildCacheRequest(automation).Activate())
        {
            var mainWindow = app.GetMainWindow(automation);
            return TreeDumper.Dump(mainWindow);
        }
    }

    private ModelDocument BuildModel(string? processName, int? pid)
    {
        var automation = GetOrCreateAutomation();
        var app = pid.HasValue ? ProcessAttacher.AttachByPid(pid.Value) : ProcessAttacher.AttachByName(processName!);
        var resolvedProcessName = processName ?? app.Name;

        using (ModelBuilder.BuildCacheRequest(automation).Activate())
        {
            var mainWindow = app.GetMainWindow(automation);
            return ModelBuilder.Build(mainWindow, resolvedProcessName);
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

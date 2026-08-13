using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace Puppet.Core;

/// <summary>
/// Owns one target application process and its current state, for the
/// interactive checkpoint-based authoring loop: the palette shown to the
/// user is derived from whatever state the app is in right now, not from
/// an accumulated model.
///
/// One session, one process - starting or resetting always kills whatever
/// is currently running first. Replay always restarts from scratch and
/// walks the flow from the top; it never continues from the current
/// state, since determinism matters more than speed here.
/// </summary>
public sealed class AppSession : IDisposable
{
    private readonly UiaThread thread = new();
    private readonly InteractionResolver interactionResolver = new();
    private UIA3Automation? automation;
    private string? exePath;
    private Application? app;
    private AutomationElement? mainWindow;
    private ModelDocument? currentModel;

    public Task<ModelDocument> StartAsync(string exePath) => thread.InvokeAsync(() => Start(exePath));

    public Task<ModelDocument> ResetAsync() => thread.InvokeAsync(Reset);

    public Task<ReplayResult> ReplayAsync(Flow flow) => thread.InvokeAsync(() => Replay(flow));

    public Task<ModelDocument> CurrentAsync() => thread.InvokeAsync(Current);

    private ModelDocument Start(string exePath)
    {
        this.exePath = exePath;
        KillIfRunning();
        LaunchAndScan();
        return currentModel!;
    }

    private ModelDocument Reset()
    {
        EnsureStarted();
        KillIfRunning();
        LaunchAndScan();
        return currentModel!;
    }

    private ReplayResult Replay(Flow flow)
    {
        EnsureStarted();
        KillIfRunning();
        LaunchAndScan();

        var results = new List<StepResult>();
        var stopped = false;

        foreach (var step in flow.Steps)
        {
            if (stopped)
            {
                results.Add(new StepResult { Description = step.Description, Status = StepStatus.Skipped });
                continue;
            }

            if (app!.HasExited)
            {
                results.Add(new StepResult
                {
                    Description = step.Description,
                    Status = StepStatus.Failed,
                    Message = "Target process exited unexpectedly.",
                });
                stopped = true;
                continue;
            }

            var stopwatch = Stopwatch.StartNew();
            var element = Waits.Poll(
                () => ResolveTarget(step) is { } e && IsReady(e) ? e : null,
                Waits.DefaultTimeout);
            var result = interactionResolver.Execute(element, step.Action);
            stopwatch.Stop();

            results.Add(new StepResult
            {
                Description = step.Description,
                Status = result.Success ? StepStatus.Passed : StepStatus.Failed,
                Duration = stopwatch.Elapsed,
                Mechanism = result.Mechanism,
                Confidence = result.Confidence,
                FailureCause = result.FailureCause,
            });

            if (!result.Success)
            {
                stopped = true;
            }
        }

        if (!app!.HasExited)
        {
            RescanCurrent();
        }

        return new ReplayResult { StepResults = results, Model = currentModel! };
    }

    private ModelDocument Current()
    {
        EnsureStarted();
        return currentModel!;
    }

    /// <summary>AutomationId first, structural path as fallback (see FlowStep).</summary>
    private AutomationElement? ResolveTarget(FlowStep step)
    {
        if (!string.IsNullOrEmpty(step.AutomationId))
        {
            var byAutomationId = ElementPathResolver.ResolveByAutomationId(mainWindow!, step.AutomationId);
            if (byAutomationId != null)
            {
                return byAutomationId;
            }
        }

        return ElementPathResolver.Resolve(mainWindow!, step.TargetPath);
    }

    private static bool IsReady(AutomationElement element) => element.Properties.IsEnabled.ValueOrDefault;

    private void LaunchAndScan()
    {
        var automationInstance = GetOrCreateAutomation();
        app = Application.Launch(exePath!);
        mainWindow = app.GetMainWindow(automationInstance);
        RescanCurrent();
    }

    private void RescanCurrent()
    {
        var automationInstance = GetOrCreateAutomation();
        var processName = Path.GetFileNameWithoutExtension(exePath!);

        using (ModelBuilder.BuildCacheRequest(automationInstance).Activate())
        {
            var cachedRoot = app!.GetMainWindow(automationInstance);
            currentModel = ModelBuilder.Build(cachedRoot, processName);
        }
    }

    private void KillIfRunning()
    {
        if (app != null && !app.HasExited)
        {
            app.Kill();
        }

        app = null;
        mainWindow = null;
    }

    private void EnsureStarted()
    {
        if (exePath == null)
        {
            throw new InvalidOperationException("Session has not been started yet.");
        }
    }

    private UIA3Automation GetOrCreateAutomation() => automation ??= new UIA3Automation();

    public void Dispose()
    {
        thread.InvokeAsync(() =>
        {
            KillIfRunning();
            automation?.Dispose();
            return true;
        }).GetAwaiter().GetResult();
        thread.Dispose();
    }
}

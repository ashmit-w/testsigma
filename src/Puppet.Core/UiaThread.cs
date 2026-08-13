using System.Collections.Concurrent;

namespace Puppet.Core;

/// <summary>
/// The single dedicated STA thread that all UI Automation calls run on.
/// Callers reach it through this async queue; nothing else may touch UIA.
/// </summary>
public sealed class UiaThread : IDisposable
{
    private readonly BlockingCollection<Action> queue = new();
    private readonly Thread thread;

    public UiaThread()
    {
        thread = new Thread(RunLoop) { IsBackground = true, Name = "Puppet-UIA" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    private void RunLoop()
    {
        foreach (var action in queue.GetConsumingEnumerable())
        {
            action();
        }
    }

    public Task<T> InvokeAsync<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.Add(() =>
        {
            try
            {
                tcs.SetResult(func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    public void Dispose()
    {
        queue.CompleteAdding();
        thread.Join();
        queue.Dispose();
    }
}

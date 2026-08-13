namespace Puppet.Core;

/// <summary>
/// The one bounded polling helper (TR-4). All waiting in Puppet goes
/// through this instead of a fixed-duration Thread.Sleep/Task.Delay.
/// </summary>
public static class Waits
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>Polls <paramref name="probe"/> until it returns non-null, or the timeout elapses.</summary>
    public static T? Poll<T>(Func<T?> probe, TimeSpan timeout) where T : class
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            var result = probe();
            if (result != null)
            {
                return result;
            }

            if (DateTime.UtcNow >= deadline)
            {
                return null;
            }

            Thread.Sleep(PollInterval);
        }
    }
}

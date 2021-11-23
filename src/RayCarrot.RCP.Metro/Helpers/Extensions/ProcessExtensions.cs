using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="Process"/>
/// </summary>
public static class ProcessExtensions
{
    /// <summary>
    /// Waits asynchronously for the process to exit
    /// </summary>
    /// <param name="process">The process to wait for to exit</param>
    /// <param name="cancellationToken">A cancellation token. If invoked, the task will return immediately as canceled.</param>
    /// <returns>A Task representing waiting for the process to end</returns>
    public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
        // Return if the process has already exited
        if (process.HasExited)
            return Task.CompletedTask;

        // Create a task completion source
        var tcs = new TaskCompletionSource<object>();

        // Subscribe for when the process exits
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => tcs.TrySetResult(null);

        if (cancellationToken == default)
            cancellationToken.Register(() => tcs.SetCanceled());

        return process.HasExited ? Task.CompletedTask : tcs.Task;
    }
}
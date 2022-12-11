using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="Process"/>
/// </summary>
public static class ProcessExtensions
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

    public static bool Is64Bit(this Process process)
    {
        if (!Environment.Is64BitOperatingSystem)
            return false;

        if (!IsWow64Process(process.Handle, out bool isWow64))
            throw new Win32Exception();

        return !isWow64;
    }

    /// <summary>
    /// Waits asynchronously for the process to exit
    /// </summary>
    /// <param name="process">The process to wait for to exit</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A Task representing waiting for the process to end</returns>
    public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
        // Return if the process has already exited
        if (process.HasExited)
            return Task.CompletedTask;

        // Create a task completion source
        var tcs = new TaskCompletionSource<object?>();

        // Subscribe for when the process exits
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => tcs.TrySetResult(null);

        cancellationToken.Register(() => tcs.SetCanceled());

        return process.HasExited ? Task.CompletedTask : tcs.Task;
    }
}
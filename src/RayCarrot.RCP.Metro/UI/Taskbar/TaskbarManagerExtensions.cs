#nullable disable
using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Windows;
using System.Windows.Interop;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="TaskbarManager"/>
/// </summary>
public static class TaskbarManagerExtensions
{
    /// <summary>
    /// Displays or updates a progress bar hosted in a taskbar button of the
    /// given window handle to show the specific percentage completed of the full operation.
    /// </summary>
    /// <param name="taskbarManager">The taskbar manager to use for displaying the progress</param>
    /// <param name="progress">The progress to display</param>
    /// <param name="windowHandle">The handle of the window whose associated taskbar button is being used as a progress
    /// indicator. This window belong to a calling process associated with the button's
    /// application and must be already loaded.</param>
    public static void SetProgressValue(this TaskbarManager taskbarManager, Progress progress, IntPtr windowHandle)
    {
        taskbarManager.SetProgressValue(((int)progress.Percentage_100).Clamp(0, 100), 100, windowHandle);
    }

    /// <summary>
    /// Displays or updates a progress bar hosted in a taskbar button of the given WPF
    /// window to show the specific percentage completed of the full operation.
    /// </summary>
    /// <param name="taskbarManager">The taskbar manager to use for displaying the progress</param>
    /// <param name="progress">The progress to display</param>
    /// <param name="window">The window whose associated taskbar button is being used as a progress indicator.
    /// This window belong to a calling process associated with the button's application
    /// and must be already loaded.</param>
    public static void SetProgressValue(this TaskbarManager taskbarManager, Progress progress, Window window)
    {
        SetProgressValue(taskbarManager, progress, new WindowInteropHelper(window).Handle);
    }
}
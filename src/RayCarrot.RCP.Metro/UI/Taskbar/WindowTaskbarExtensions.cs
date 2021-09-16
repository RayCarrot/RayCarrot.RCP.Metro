using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows;
using System.Windows.Interop;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="Window"/>
    /// </summary>
    public static class WindowTaskbarExtensions
    {
        /// <summary>
        /// Displays or updates a progress bar hosted in a taskbar button of the given WPF
        /// window to show the specific percentage completed of the full operation.
        /// </summary>
        /// <param name="window">The window whose associated taskbar button is being used as a progress indicator.
        /// This window belong to a calling process associated with the button's application
        /// and must be already loaded.</param>
        /// <param name="currentValue">An application-defined value that indicates the proportion of the operation that
        /// has been completed at the time the method is called.</param>
        /// <param name="maximumValue">An application-defined value that specifies the value currentValue will have
        /// when the operation is complete.</param>
        public static void SetTaskbarProgressValue(this Window window, int currentValue, int maximumValue)
        {
            TaskbarManager.Instance.SetProgressValue(currentValue, maximumValue, new WindowInteropHelper(window).Handle);
        }

        /// <summary>
        /// Displays or updates a progress bar hosted in a taskbar button of the given WPF
        /// window to show the specific percentage completed of the full operation.
        /// </summary>
        /// <param name="window">The window whose associated taskbar button is being used as a progress indicator.
        /// This window belong to a calling process associated with the button's application
        /// and must be already loaded.</param>
        /// <param name="progress">The progress to display</param>
        public static void SetTaskbarProgressValue(this Window window, Progress progress)
        {
            TaskbarManager.Instance.SetProgressValue(progress, new WindowInteropHelper(window).Handle);
        }

        /// <summary>
        /// Sets the type and state of the progress indicator displayed on a taskbar button
        /// of the given WPF window
        /// </summary>
        /// <param name="window">The window whose associated taskbar button is being used as a progress indicator.
        /// This window belong to a calling process associated with the button's application
        /// and must be already loaded.</param>
        /// <param name="taskbarProgressBar">Progress state of the progress button</param>
        public static void SetTaskbarProgressState(this Window window, TaskbarProgressBarState taskbarProgressBar)
        {
            TaskbarManager.Instance.SetProgressState(taskbarProgressBar, new WindowInteropHelper(window).Handle);
        }
    }
}
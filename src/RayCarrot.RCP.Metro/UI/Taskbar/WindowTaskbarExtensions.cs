using Microsoft.WindowsAPICodePack.Taskbar;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="Window"/>
/// </summary>
public static class WindowTaskbarExtensions
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

    private static FLASHWINFO CreateFlashInfo(IntPtr handle, FlashFlags flags, uint count, uint timeout)
    {
        FLASHWINFO fi = new();
        fi.cbSize = (uint)Marshal.SizeOf(fi);
        fi.hwnd = handle;
        fi.dwFlags = flags;
        fi.uCount = count;
        fi.dwTimeout = timeout;
        return fi;
    }

    /// <summary>
    /// Flash the specified window until it receives focus.
    /// </summary>
    /// <param name="window">The window to flash.</param>
    /// <returns></returns>
    public static bool Flash(this Window window)
    {
        IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        FLASHWINFO fi = CreateFlashInfo(windowHandle, FlashFlags.FLASHW_ALL | FlashFlags.FLASHW_TIMERNOFG, UInt32.MaxValue, 0);
        return FlashWindowEx(ref fi);
    }

    /// <summary>
    /// Flash the specified window for a specific number of times.
    /// </summary>
    /// <param name="window">The window to flash.</param>
    /// <param name="count">The number of times to flash.</param>
    /// <param name="onlyFlashIfNotInForeground">Indicates if the window should only flash if not in the foreground.</param>
    /// <returns></returns>
    public static bool Flash(this Window window, uint count, bool onlyFlashIfNotInForeground)
    {
        FlashFlags flags = FlashFlags.FLASHW_ALL;

        if (onlyFlashIfNotInForeground)
            flags |= FlashFlags.FLASHW_TIMERNOFG;

        IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        FLASHWINFO fi = CreateFlashInfo(windowHandle, flags, count, 0);
        return FlashWindowEx(ref fi);
    }

    /// <summary>
    /// Start flashing the specified window.
    /// </summary>
    /// <param name="window">The window to flash.</param>
    /// <returns></returns>
    public static bool StartFlashing(this Window window)
    {
        IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        FLASHWINFO fi = CreateFlashInfo(windowHandle, FlashFlags.FLASHW_ALL, UInt32.MaxValue, 0);
        return FlashWindowEx(ref fi);
    }

    /// <summary>
    /// Stop flashing the specified window.
    /// </summary>
    /// <param name="window">The window to stop flashing.</param>
    /// <returns></returns>
    public static bool StopFlashing(this Window window)
    {
        IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        FLASHWINFO fi = CreateFlashInfo(windowHandle, FlashFlags.FLASHW_STOP, UInt32.MaxValue, 0);
        return FlashWindowEx(ref fi);
    }

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

    [StructLayout(LayoutKind.Sequential)]
    private struct FLASHWINFO
    {
        /// <summary>
        /// The size of the structure in bytes.
        /// </summary>
        public uint cbSize;

        /// <summary>
        /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
        /// </summary>
        public IntPtr hwnd;

        /// <summary>
        /// The Flash Status.
        /// </summary>
        public FlashFlags dwFlags;

        /// <summary>
        /// The number of times to Flash the window.
        /// </summary>
        public uint uCount;

        /// <summary>
        /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
        /// </summary>
        public uint dwTimeout;
    }

    [Flags]
    private enum FlashFlags : uint
    {
        /// <summary>
        /// Stop flashing. The system restores the window to its original state.
        /// </summary>
        FLASHW_STOP = 0,

        /// <summary>
        /// Flash the window caption.
        /// </summary>
        FLASHW_CAPTION = 1,

        /// <summary>
        /// Flash the taskbar button.
        /// </summary>
        FLASHW_TRAY = 2,

        /// <summary>
        /// Flash both the window caption and taskbar button. This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        FLASHW_ALL = FLASHW_CAPTION | FLASHW_TRAY,

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        FLASHW_TIMER = 4,

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        FLASHW_TIMERNOFG = 12,
    }
}
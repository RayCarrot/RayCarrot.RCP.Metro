using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Contains information regarding the state of a Window
/// </summary>
public class WindowSessionState
{
    #region Public Properties

    /// <summary>
    /// The top position
    /// </summary>
    public double WindowTop { get; set; }

    /// <summary>
    /// The left position
    /// </summary>
    public double WindowLeft { get; set; }

    /// <summary>
    /// The height
    /// </summary>
    public double WindowHeight { get; set; }

    /// <summary>
    /// The width
    /// </summary>
    public double WindowWidth { get; set; }

    /// <summary>
    /// True if maximized
    /// </summary>
    public bool WindowMaximized { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Applies the current state to the specified <see cref="Window"/>
    /// </summary>
    /// <param name="window">The window to apply the state to</param>
    public void ApplyToWindow(Window window)
    {
        Rect virtualScreenRect = new(
            x: SystemParameters.VirtualScreenLeft,
            y: SystemParameters.VirtualScreenTop,
            width: SystemParameters.VirtualScreenWidth,
            height: SystemParameters.VirtualScreenHeight);

        double width = WindowWidth;
        double height = WindowHeight;
        double x = WindowLeft;
        double y = WindowTop;

        if (y < virtualScreenRect.Y)
            y = virtualScreenRect.Y;

        if (x < virtualScreenRect.X)
            x = virtualScreenRect.X;

        if (x + width > virtualScreenRect.Right)
            x = virtualScreenRect.Right - width;

        if (y + height > virtualScreenRect.Bottom)
            y = virtualScreenRect.Bottom - height;

        window.Width = width;
        window.Height = height;
        window.Left = x;
        window.Top = y;
        window.WindowState = WindowMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Gets the state from a <see cref="Window"/>
    /// </summary>
    /// <param name="window">The window to get the state from</param>
    /// <returns>The state</returns>
    public static WindowSessionState GetWindowState(Window window)
    {
        return new WindowSessionState()
        {
            WindowHeight = window.Height,
            WindowWidth = window.Width,
            WindowLeft = window.Left,
            WindowTop = window.Top,
            WindowMaximized = window.WindowState == WindowState.Maximized
        };
    }

    #endregion
}
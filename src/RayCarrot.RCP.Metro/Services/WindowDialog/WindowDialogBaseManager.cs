using System.Windows;
using System.Windows.Threading;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Default dialog base manager, showing the dialog in a <see cref="Window"/>
/// </summary>
public class WindowDialogBaseManager : IDialogBaseManager
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Protected Properties

    protected HashSet<OpenWindowInstance> OpenWindows { get; } = new();

    #endregion

    #region Protected Methods

    protected Dispatcher GetDispatcher()
    {
        return Application.Current?.Dispatcher ?? throw new Exception("A window can not be created before the application has been loaded and the dispatcher set");
    }

    protected void ConfigureWindow(Window window, IWindowControl windowContent)
    {
        // Set window properties
        window.Content = windowContent.UIContent;
        window.ResizeMode = windowContent.IsResizable ? ResizeMode.CanResize : ResizeMode.NoResize;
        window.SizeToContent = windowContent.IsResizable ? SizeToContent.Manual : SizeToContent.WidthAndHeight;

        if (!windowContent.IsResizable)
        {
            window.MinWidth = 50;
            window.MinHeight = 50;
        }

        // Set startup location
        window.WindowStartupLocation = window.Owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
    }

    protected virtual async Task ShowAsync(IWindowControl windowContent, bool isModal, string? title)
    {
        // Create the window
        BaseIconWindow window = new();

        // Configure the window
        ConfigureWindow(window, windowContent);

        // Set the window instance
        windowContent.WindowInstance = new StandardWindowInstance(window);

        // Set the title
        if (title != null)
            windowContent.WindowInstance.Title = title;

        if (isModal)
        {
            // Show the window as a dialog
            window.ShowDialog();
        }
        else
        {
            TaskCompletionSource<object?> tcs = new();

            void Window_Closed(object sender, EventArgs e)
            {
                window.Closed -= Window_Closed;
                tcs.TrySetResult(null);
            }

            window.Closed += Window_Closed;

            // Show the window
            window.Show();

            // Wait for the window to close
            await tcs.Task;
        }
    }

    #endregion

    #region Public Methods

    public async Task<ShowWindowResult> ShowWindowAsync(
        IWindowControl windowContent, 
        ShowWindowFlags flags = ShowWindowFlags.None, 
        string[]? typeGroupNames = null, 
        string[]? globalGroupNames = null,
        string? title = null)
    {
        using (windowContent)
        {
            OpenWindowInstance openWindowInstance = new(windowContent, typeGroupNames, globalGroupNames);

            try
            {
                // Check for blocking window
                if (flags.HasFlag(ShowWindowFlags.DuplicateTypesNotAllowed) ||
                    (globalGroupNames != null && globalGroupNames.Any()) ||
                    (typeGroupNames != null && typeGroupNames.Any()))
                {
                    Type contentType = windowContent.UIContent.GetType();

                    // Get first potentially blocking window
                    IWindowControl? blockingWindow = OpenWindows.
                        FirstOrDefault(x =>
                        {
                            // Check for duplicate types
                            if (flags.HasFlag(ShowWindowFlags.DuplicateTypesNotAllowed) && x.Window.UIContent.GetType() == contentType)
                                return true;

                            // Check for duplicate global group names
                            if (globalGroupNames != null &&
                                globalGroupNames.Any() &&
                                x.GlobalGroupNames != null &&
                                x.GlobalGroupNames.Any(globalGroupNames.Contains))
                                return true;

                            // Check for duplicate type group names
                            if (typeGroupNames != null &&
                                typeGroupNames.Any() &&
                                x.Window.UIContent.GetType() == contentType &&
                                x.TypeGroupNames != null &&
                                x.TypeGroupNames.Any(typeGroupNames.Contains))
                                return true;

                            return false;
                        })?.Window;

                    // If there is a window blocking this one from showing we return
                    if (blockingWindow != null)
                    {
                        Logger.Info("The window is not being shown due to a window of the same type or ID being available");

                        // Focus the blocking window
                        if (!flags.HasFlag(ShowWindowFlags.DoNotFocusBlockingWindow))
                            blockingWindow.WindowInstance?.Focus();

                        return new ShowWindowResult(false, blockingWindow);
                    }
                }

                OpenWindows.Add(openWindowInstance);

                // Get the dispatcher
                Dispatcher dispatcher = GetDispatcher();

                // Show the window and wait for it to close
                await dispatcher.Invoke(() => ShowAsync(windowContent, flags.HasFlag(ShowWindowFlags.Modal), title));

                return new ShowWindowResult(true, null);
            }
            finally
            {
                // Remove the window from list of open windows
                OpenWindows.Remove(openWindowInstance);
            }
        }
    }

    #endregion

    #region Records

    protected record OpenWindowInstance(IWindowControl Window, string[]? TypeGroupNames, string[]? GlobalGroupNames);

    #endregion
}
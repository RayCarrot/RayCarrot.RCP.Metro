using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    protected HashSet<OpenWindowInstance> OpenWindows { get; } = new HashSet<OpenWindowInstance>();

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

    protected Task ShowWindowAsync(IWindowControl windowContent, bool isModal, string title)
    {
        // Get the dispatcher
        Dispatcher dispatcher = GetDispatcher();

        // Run on UI thread
        return dispatcher.Invoke(() => ShowAsync(windowContent, isModal, title));
    }

    protected virtual async Task ShowAsync(IWindowControl windowContent, bool isModal, string title)
    {
        // Create the window
        var window = new BaseIconWindow();

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
            var tcs = new TaskCompletionSource<object>();

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

    public async Task<Result> ShowDialogWindowAsync<UserInput, Result>(IDialogWindowControl<UserInput, Result> windowContent)
        where UserInput : UserInputViewModel
        where Result : UserInputResult
    {
        try
        {
            // Show as a modal with the user input title
            await ShowWindowAsync(windowContent, true, windowContent.ViewModel.Title);

            // Get the dispatcher
            Dispatcher dispatcher = GetDispatcher();

            // Return the result
            return dispatcher.Invoke(windowContent.GetResult);
        }
        finally
        {
            windowContent.Dispose();
        }
    }

    public async Task ShowWindowAsync(IWindowControl windowContent, ShowWindowFlags flags = ShowWindowFlags.None, params string[] groupNames)
    {
        var openWindowInstance = new OpenWindowInstance(new WeakReference<IWindowControl>(windowContent), groupNames);

        try
        {
            // Remove instances which have been collected by the GC
            OpenWindows.RemoveWhere(x => !x.Window.TryGetTarget(out _));

            Type contentType = windowContent.UIContent.GetType();

            IWindowControl blockingWindow = OpenWindows.
                Select(x => x.Window.TryGetTarget(out IWindowControl w) ? new { Window = w, x.GroupNames } : null).
                FirstOrDefault(x =>
                {
                    // Check for duplicate types
                    if (!flags.HasFlag(ShowWindowFlags.DuplicateTypesAllowed) && x?.Window.UIContent.GetType() == contentType)
                        return true;

                    // Check for duplicate group names
                    if (groupNames.Any() && x?.GroupNames.Any(groupNames.Contains) == true)
                        return true;

                    return false;
                })?.Window;

            // If there is a window blocking this one from showing we return
            if (blockingWindow != null)
            {
                Logger.Info("The window is not being shown due to a window of the same type or ID being available", contentType);

                // Focus the blocking window
                if (!flags.HasFlag(ShowWindowFlags.DoNotFocusBlockingWindow))
                    blockingWindow.WindowInstance?.Focus();

                return;
            }

            OpenWindows.Add(openWindowInstance);

            // Show the window
            await ShowWindowAsync(windowContent, false, null);
        }
        finally
        {
            OpenWindows.Remove(openWindowInstance);

            windowContent.Dispose();
        }
    }

    #endregion

    #region Records

    protected record OpenWindowInstance(WeakReference<IWindowControl> Window, string[] GroupNames);

    #endregion
}
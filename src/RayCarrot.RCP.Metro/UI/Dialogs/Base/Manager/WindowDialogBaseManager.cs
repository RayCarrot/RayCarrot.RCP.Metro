using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Default dialog base manager, showing the dialog in a <see cref="Window"/>
    /// </summary>
    public class WindowDialogBaseManager : IDialogBaseManager
    {
        /// <summary>
        /// Shows the dialog and returns when the dialog closes with a result
        /// </summary>
        /// <typeparam name="V">The view model type</typeparam>
        /// <typeparam name="R">The return type</typeparam>
        /// <param name="dialog">The dialog to show</param>
        /// <param name="owner">The owner window</param>
        /// <returns>The result</returns>
        public async Task<R> ShowDialogAsync<V, R>(IDialogBaseControl<V, R> dialog, object owner)
            where V : UserInputViewModel
        {
            using (dialog)
            {
                // Get the dispatcher
                var dispatcher = Application.Current.Dispatcher;

                // Make sure the dispatcher is not null
                if (dispatcher == null)
                    throw new Exception("A dialog can not be created before the application has been loaded");

                // Run on UI thread
                return await dispatcher.Invoke(async () =>
                {
                    // Get the parent window
                    if (Services.Data.UI_UseChildWindows && GetChildWindowParent() is MetroWindow metroWindow)
                    {
                        // Create the child window
                        var childWin = new ChildWindow();

                        // Configure the window
                        ConfigureChildWindow(childWin, dialog, owner);

                        void Dialog_CloseDialog(object sender, EventArgs e) => childWin.Close();
                        static void Window_Closing(object sender, CancelEventArgs e) => e.Cancel = true;

                        var wasCloseEnabled = metroWindow.IsCloseButtonEnabled;
                        metroWindow.Closing += Window_Closing;
                        dialog.CloseDialog += Dialog_CloseDialog;
                        metroWindow.IsCloseButtonEnabled = false;

                        await metroWindow.ShowChildWindowAsync(childWin);

                        metroWindow.Closing -= Window_Closing;
                        dialog.CloseDialog -= Dialog_CloseDialog;
                        metroWindow.IsCloseButtonEnabled = wasCloseEnabled;
                    }
                    else
                    {
                        // If there is no parent window we can't display the dialog as a child window, so fall back to a normal window
                        var window = GetWindow();

                        ConfigureWindow(window, dialog, owner);

                        void DialogWin_CloseDialog(object sender, EventArgs e) => window.Close();

                        // Close window on request
                        dialog.CloseDialog += DialogWin_CloseDialog;

                        // Show window as dialog
                        window.ShowDialog();

                        // Unsubscribe
                        dialog.CloseDialog -= DialogWin_CloseDialog;
                    }

                    // Return the result
                    return dialog.GetResult();
                });
            }
        }

        /// <summary>
        /// Shows the Window without waiting for it to close
        /// </summary>
        /// <typeparam name="VM">The view model</typeparam>
        /// <param name="windowContent">The window content to show</param>
        /// <param name="owner">The owner window</param>
        /// <returns>The window</returns>
        public Task<Window> ShowWindowAsync<VM>(IWindowBaseControl<VM> windowContent, object owner)
            where VM : UserInputViewModel
        {
            lock (Application.Current)
            {
                // Get the dispatcher
                var dispatcher = Application.Current.Dispatcher;

                // Make sure the dispatcher is not null
                if (dispatcher == null)
                    throw new Exception("A dialog can not be created before the application has been loaded");

                // Run on UI thread
                return Task.FromResult(dispatcher.Invoke(() =>
                {
                    // Create the window
                    var window = GetWindow();

                    // Configure the window
                    ConfigureWindow(window, windowContent, owner);

                    void Dialog_CloseDialog(object sender, EventArgs e)
                    {
                        window.Close();
                    }

                    void Dialog_Closed(object sender, EventArgs e)
                    {
                        windowContent.CloseDialog -= Dialog_CloseDialog;
                        window.Closed -= Dialog_Closed;
                    }

                    // Close window on request
                    windowContent.CloseDialog += Dialog_CloseDialog;
                    window.Closed += Dialog_Closed;

                    // Show the window
                    AppWindowsManager.ShowWindow(window, AppWindowsManager.ShowWindowFlags.DuplicatesAllowed, windowContent.GetType().FullName);

                    // Return the window
                    return window;
                }));
            }
        }

        /// <summary>
        /// Creates a new window with the specified content
        /// </summary>
        /// <typeparam name="VM">The view model</typeparam>
        /// <param name="window">The window to configure</param>
        /// <param name="windowContent">The window content to show</param>
        /// <param name="owner">The owner window</param>
        public virtual void ConfigureWindow<VM>(Window window, IWindowBaseControl<VM> windowContent, object owner)
            where VM : UserInputViewModel
        {
            // Set window properties
            window.Content = windowContent.UIContent;
            window.ResizeMode = windowContent.Resizable ? ResizeMode.CanResize : ResizeMode.NoResize;
            window.Title = windowContent.ViewModel.Title ?? String.Empty;
            window.SizeToContent = windowContent.Resizable ? SizeToContent.Manual : SizeToContent.WidthAndHeight;

            // Set size properties
            switch (windowContent.BaseSize)
            {
                case DialogBaseSize.Smallest:
                    if (windowContent.Resizable)
                    {
                        window.Height = 100;
                        window.Width = 150;
                    }

                    window.MinHeight = 100;
                    window.MinWidth = 150;

                    break;

                case DialogBaseSize.Small:
                    if (windowContent.Resizable)
                    {
                        window.Height = 200;
                        window.Width = 250;
                    }

                    window.MinHeight = 200;
                    window.MinWidth = 250;

                    break;

                case DialogBaseSize.Medium:
                    if (windowContent.Resizable)
                    {
                        window.Height = 350;
                        window.Width = 500;
                    }

                    window.MinHeight = 300;
                    window.MinWidth = 400;

                    break;

                case DialogBaseSize.Large:
                    if (windowContent.Resizable)
                    {
                        window.Height = 475;
                        window.Width = 750;
                    }

                    window.MinHeight = 350;
                    window.MinWidth = 500;

                    break;

                case DialogBaseSize.Largest:
                    if (windowContent.Resizable)
                    {
                        window.Height = 600;
                        window.Width = 900;
                    }

                    window.MinHeight = 500;
                    window.MinWidth = 650;

                    break;
            }

            // Set owner
            if (owner is Window ow)
                window.Owner = ow;
            else if (owner is IntPtr oi)
                new WindowInteropHelper(window).Owner = oi;
            else
                window.Owner = Application.Current?.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive);

            // Set startup location
            window.WindowStartupLocation = window.Owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
        }

        /// <summary>
        /// Gets the parent window to use for showing the child window. If this returns null then no suitable window was found.
        /// </summary>
        /// <returns>The parent window, or null if none was found</returns>
        public Window GetChildWindowParent()
        {
            // Start by checking if any windows are active, in which case we use that
            Window activeWin = Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive);

            if (activeWin != null)
                return activeWin;

            // If no windows are active we check if there is a modal window
            Window modalWin = Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsModal());

            if (modalWin != null)
                return modalWin;

            // If no windows are active or modal we try and get the main window
            Window mainWin = Application.Current.MainWindow;

            // Return the window. It might be null (if the application hasn't fully started) in which case no window was found.
            return mainWin;
        }

        public void ConfigureChildWindow<VM>(ChildWindow window, IWindowBaseControl<VM> windowContent, object owner)
            where VM : UserInputViewModel
        {
            // Set window properties
            window.Content = windowContent.UIContent;
            window.Title = windowContent.ViewModel.Title ?? String.Empty;

            if (windowContent.Resizable)
            {
                window.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                window.VerticalContentAlignment = VerticalAlignment.Stretch;
            }
        }

        /// <summary>
        /// Gets a new instance of a window
        /// </summary>
        /// <returns>The window instance</returns>
        public virtual Window GetWindow()
        {
            return new Window();
        }
    }
}
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using RayCarrot.Common;

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
                    // Create the window
                    var childWin = new ChildWindow();

                    // Configure the window
                    ConfigureChildWindow(childWin, dialog, owner);

                    void Dialog_CloseDialog(object sender, EventArgs e) => childWin.Close();
                    static void Window_Closing(object sender, CancelEventArgs e) => e.Cancel = true;

                    // Get the parent window
                    var window = (MetroWindow)(Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow);

                    var wasCloseEnabled = window.IsCloseButtonEnabled;
                    window.Closing += Window_Closing;
                    dialog.CloseDialog += Dialog_CloseDialog;
                    window.IsCloseButtonEnabled = false;

                    await window.ShowChildWindowAsync(childWin);

                    window.Closing -= Window_Closing;
                    dialog.CloseDialog -= Dialog_CloseDialog;
                    window.IsCloseButtonEnabled = wasCloseEnabled;

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
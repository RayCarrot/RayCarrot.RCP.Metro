﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Default dialog base manager, showing the dialog in a <see cref="Window"/>
    /// </summary>
    public class WindowDialogBaseManager : IDialogBaseManager
    {
        #region Protected Methods

        protected static void ConfigureWindow(Window window, IWindowControl windowContent)
        {
            // Set window properties
            window.Content = windowContent.UIContent;
            window.ResizeMode = windowContent.ResizeMode == IWindowControl.WindowResizeMode.NoResize
                ? ResizeMode.NoResize
                : ResizeMode.CanResize;
            window.SizeToContent = windowContent.ResizeMode == IWindowControl.WindowResizeMode.NoResize
                ? SizeToContent.WidthAndHeight
                : SizeToContent.Manual;

            // Set startup location
            window.WindowStartupLocation = window.Owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
        }

        protected Task ShowWindowAsync(IWindowControl windowContent, bool isModal, string title)
        {
            // Get the dispatcher
            Dispatcher dispatcher = Application.Current?.Dispatcher ?? throw new Exception("A window can not be created before the application has been loaded and the dispatcher set");

            // Run on UI thread
            return dispatcher.Invoke(() => ShowAsync(windowContent, isModal, title));
        }

        protected virtual async Task ShowAsync(IWindowControl windowContent, bool isModal, string title)
        {
            // Create the window
            var window = new BaseWindow();

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
            // Show as a modal with the user input title
            await ShowWindowAsync(windowContent, true, windowContent.ViewModel.Title);

            // Return the result
            return windowContent.GetResult();
        }

        public Task ShowWindowAsync(IWindowControl windowContent)
        {
            // Show the window
            return ShowWindowAsync(windowContent, false, null);
        }

        #endregion
    }
}
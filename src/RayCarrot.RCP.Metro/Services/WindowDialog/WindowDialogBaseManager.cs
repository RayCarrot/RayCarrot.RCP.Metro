using System;
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

        public Task ShowWindowAsync(IWindowControl windowContent)
        {
            try
            {
                // Show the window
                return ShowWindowAsync(windowContent, false, null);
            }
            finally
            {
                windowContent.Dispose();
            }
        }

        #endregion
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Updater
{
    /// <summary>
    /// View model for the updater
    /// </summary>
    public class UpdaterViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UpdaterViewModel()
        {
            // Set total bytes to 1 so the progress bar is not maxed out
            TotalBytes = 1;

            CanCancel = false;

            CancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs the update
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The task</returns>
        public async Task UpdateAsync(CancellationToken cancellationToken)
        {
            // Make sure the update hasn't started
            if (App.Stage > UpdateStage.None)
                return;

            CanCancel = true;

            App.Stage = UpdateStage.Initial;

            AddLog($"App path retrieved as {App.RCPFilePath}", UserLevel.Debug);

            // Get the file URL
            ServerFileURL = App.UpdateURL;
            AddLog($"Server file URL detected as {ServerFileURL}", UserLevel.Debug);

            AddLog($"Temp download path generated as {App.ServerTempPath}", UserLevel.Debug);

            // Subscribe to when progress is changed
            App.WC.DownloadProgressChanged += (s, e) =>
            {
                TotalBytes = e.TotalBytesToReceive;
                ReceivedBytes = e.BytesReceived;
            };

            cancellationToken.Register(App.WC.CancelAsync);

            try
            {
                AddLog("Downloading...");

                App.Stage = UpdateStage.Download;

                // Download the file async
                await App.WC.DownloadFileTaskAsync(new Uri(ServerFileURL), App.ServerTempPath);
            }
            catch (Exception ex)
            {
                if (ex is WebException we && we.Status == WebExceptionStatus.RequestCanceled)
                {
                    App.ShutdownApplication("Canceled", ex);
                    return;
                }

                MessageBox.Show($"An error occurred while downloading the update", $"Download failed", MessageBoxButton.OK, MessageBoxImage.Error);

                App.ShutdownApplication("Downloading update", ex);
                return;
            }

            AddLog("Downloaded successfully");

            if (cancellationToken.IsCancellationRequested)
            {
                App.ShutdownApplication("Canceled");
                return;
            }

            CanCancel = false;

            AddLog("Installing...");

            App.Stage = UpdateStage.Install;

            AddLog($"Temp local backup path generated as {App.LocalTempPath}", UserLevel.Debug);

            try
            {
                if (File.Exists(App.LocalTempPath))
                    File.Delete(App.LocalTempPath);

                // Move old program to temp path
                File.Move(App.RCPFilePath, App.LocalTempPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while installing the update", $"Installation failed", MessageBoxButton.OK, MessageBoxImage.Error);

                App.ShutdownApplication("Installing update - stage 1", ex);
                return;
            }

            try
            {
                if (File.Exists(App.RCPFilePath))
                    File.Delete(App.RCPFilePath);

                // Move downloaded update to old program's path
                File.Move(App.ServerTempPath, App.RCPFilePath);
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(App.RCPFilePath))
                        File.Delete(App.RCPFilePath);

                    // Try to restore local version
                    File.Move(App.LocalTempPath, App.RCPFilePath);
                }
                catch (Exception ex2)
                {
                    AddLog($"Restoring local version failed with exception: {Environment.NewLine}{ex2}");
                }

                MessageBox.Show($"An error occurred while installing the update", $"Installation failed", MessageBoxButton.OK, MessageBoxImage.Error);

                App.ShutdownApplication("Installing update - stage 2", ex);
                return;
            }

            AddLog("Installed");

            // Clear the temporary files
            App.ClearTemp();

            // Flag that the operation is finished
            App.Stage = UpdateStage.Finished;

            // Start the updated program
            Process.Start(App.RCPFilePath)?.Dispose();

            // Close the program
            App.Shutdown();
        }

        /// <summary>
        /// Cancels the ongoing update operation
        /// </summary>
        public void CancelUpdate()
        {
            if (CanCancel)
                CancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Adds a local log to the program
        /// </summary>
        /// <param name="text">The log text</param>
        /// <param name="ul">The required user level</param>
        private void AddLog(string text, UserLevel ul = UserLevel.Normal)
        {
            if (App.CurrentUserLevel >= ul)
                Log += text + Environment.NewLine;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The log
        /// </summary>
        public string Log { get; set; }

        /// <summary>
        /// The URL to download the update from
        /// </summary>
        public string ServerFileURL { get; set; }

        /// <summary>
        /// The total amount of bytes to receive
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// The amount of bytes received
        /// </summary>
        public long ReceivedBytes { get; set; }

        /// <summary>
        /// Indicates if the updating operation can be canceled in its current state
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// The cancellation token source
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; }

        #endregion
    }
}
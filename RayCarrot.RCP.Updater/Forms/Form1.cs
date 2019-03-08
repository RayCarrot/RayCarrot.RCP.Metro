using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Updater
{
    public partial class Form1 : Form
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="args">The launch arguments</param>
        public Form1(IReadOnlyList<string> args)
        {
            InitializeComponent();

            // Disable update button
            button1.Enabled = false;

            // Get file path from arguments
            if (args.Count > 0)
                FilePath = args[0];

            // Get user level from arguments
            CurrentUserLevel = args.Count > 1
                ? Enum.TryParse(args[1], out UserLevel ul) ? ul : UserLevel.Normal
                : UserLevel.Normal;

            // Get boolean indicating if the update should start automatically
            if (args.Count > 2)
                AutoUpdate = Boolean.TryParse(args[2], out bool au) && au;

            // Get temp file names
            ServerTempPath = Path.GetTempFileName();
            LocalTempPath = Path.GetTempFileName();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates if the update should start automatically
        /// </summary>
        private bool AutoUpdate { get; }

        /// <summary>
        /// The path of the program to update
        /// </summary>
        private string FilePath { get; set; }

        /// <summary>
        /// The URL to download the update from
        /// </summary>
        private string FileURL { get; set; }

        /// <summary>
        /// The current user level
        /// </summary>
        private UserLevel CurrentUserLevel { get; }

        /// <summary>
        /// True if an update operation is running, false if not
        /// </summary>
        private bool OperationRunning { get; set; }

        /// <summary>
        /// The web client from downloading the update
        /// </summary>
        private WebClient WC { get; set; }

        /// <summary>
        /// The temporary path for the server update
        /// </summary>
        private string ServerTempPath { get; }

        /// <summary>
        /// The temporary path for the local program
        /// </summary>
        private string LocalTempPath { get; }

        #endregion

        #region Event Handlers

        private async void Form1_ShownAsync(object sender, EventArgs e)
        {
            // Make sure the file exists
            if (!File.Exists(FilePath))
            {
                MessageBox.Show("Select the program file to update", "Program not found", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Allow user to select the path
                using (var fd = new OpenFileDialog
                {
                    InitialDirectory = Directory.GetCurrentDirectory(),
                    Filter = "exe files|*.exe",
                    FileName = "Rayman Control Panel.exe",
                    Title = "Select the program file to update"
                })
                {
                    // Show the dialog
                    if (fd.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("Update operation canceled", "Operation canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Application.Exit();
                        return;
                    }

                    // Make sure the file exists
                    if (!File.Exists(fd.FileName))
                    {
                        MessageBox.Show("The specified file does not exist", "Operation canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Application.Exit();
                        return;
                    }

                    FilePath = fd.FileName;
                }
            }

            AddLog($"Path detected as {FilePath}");
            AddLog("Getting server update manifest");

            string errorMessage = "Unknown error";
            JObject manifest = null;

            const string BaseUrl = "http://raycarrot.ylemnova.com/RCP/";
            const string UpdateManifestUrl = BaseUrl + "RCP_Metro_Manifest.json";

            // Attempt to get the manifest
            try
            {
                using (var wc = new WebClient())
                {
                    var result = await wc.DownloadStringTaskAsync(UpdateManifestUrl);
                    manifest = JObject.Parse(result);
                }
            }
            catch (WebException ex)
            {
                AddLog($"Exception: {Environment.NewLine}{ex}", UserLevel.Debug);
                errorMessage = "A connection could not be established to the server";
            }
            catch (JsonReaderException ex)
            {
                AddLog($"Exception: {Environment.NewLine}{ex}", UserLevel.Debug);
                errorMessage = "The information from the server was not valid";
            }
            catch (Exception ex)
            {
                AddLog($"Exception: {Environment.NewLine}{ex}", UserLevel.Debug);
                errorMessage = "An unknown error occurred while connecting to the server";
            }

            AddLog($"Manifest URL check {UpdateManifestUrl} failed", UserLevel.Debug);

            // Show error if manifest was not retrieved
            if (manifest == null)
            {
                MessageBox.Show(errorMessage);
                Application.Exit();
                return;
            }

            AddLog("Server update manifest loaded");
            AddLog($"Manifest retrieved as {manifest}", UserLevel.Debug);

            try
            {
                // Get the current version
                var localVersion = Version.Parse(FileVersionInfo.GetVersionInfo(FilePath).FileVersion);
                AddLog($"Current version detected as {localVersion}");

                // Get the server version
                var av = manifest["LatestAssemblyVersion"];
                var serverVersion = new Version(av["Major"].Value<int>(), av["Minor"].Value<int>(), av["Build"].Value<int>(), av["Revision"].Value<int>());
                AddLog($"Latest version detected as {serverVersion}", UserLevel.Advanced);

                // Compare version
                if (localVersion >= serverVersion)
                {
                    MessageBox.Show("The latest version is already installed", "No update available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return;
                }

                AddLog("New update available");
            }
            catch (Exception ex)
            {
                DownloadCancelled(UpdateStage.Manifest, ex);
                return;
            }

            try
            {
                // Get the file URL
                FileURL = manifest["URL"].Value<string>();
                AddLog($"Server file URL detected as {FileURL}", UserLevel.Debug);
            }
            catch (Exception ex)
            {
                DownloadCancelled(UpdateStage.Manifest, ex);
                return;
            }

            string news = "Error getting news";
            try
            {
                // Get the update news
                news = manifest["DisplayNews"].Value<string>();
                AddLog($"Version News: {Environment.NewLine}{String.Join("\n", news.Split('\n').Select(x => $"   {x}"))}");
            }
            catch (Exception ex)
            {
                AddLog($"Exception: {Environment.NewLine}{ex}", UserLevel.Debug);
                AddLog("Failed to get news", UserLevel.Advanced);
            }

            if (AutoUpdate)
            {
                Button1_ClickAsync(null, null);
                return;
            }

            // Enable the update button
            button1.Enabled = true;

            MessageBox.Show($"A new version is available to download.{Environment.NewLine}{Environment.NewLine}News: {Environment.NewLine}{news}", "Update available");
        }

        private void ClosingForm(object sender, FormClosingEventArgs e)
        {
            // Allow closing if no operation is running
            if (!OperationRunning)
                return;

            if (MessageBox.Show("Cancel download?", "Cancel current update download", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                // Cancel the current web client operation
                WC?.CancelAsync();
                AddLog("Cancellation requested");
            }

            e.Cancel = true;
        }

        private async void Button1_ClickAsync(object sender, EventArgs e)
        {
            // Make sure it's not running twice
            if (OperationRunning)
                return;

            // Disable the update button
            button1.Enabled = false;

            // Flag that an operation is running
            OperationRunning = true;

            AddLog($"Temp download path generated as {ServerTempPath}", UserLevel.Debug);

            try
            {
                // Create the web client
                using (WC = new WebClient())
                {
                    AddLog("Downloading update...");

                    // Subscribe to when progress is changed
                    WC.DownloadProgressChanged += Wc_DownloadProgressChanged;

                    // Download the file async
                    await WC.DownloadFileTaskAsync(new Uri(FileURL), ServerTempPath);
                }
            }
            catch (Exception ex)
            {
                DownloadCancelled(UpdateStage.Download, ex);
                return;
            }

            AddLog("Update downloaded successfully");
            AddLog("Installing update... Do not close the application!");

            AddLog($"Temp local backup path generated as {LocalTempPath}", UserLevel.Debug);

            try
            {
                if (File.Exists(LocalTempPath))
                    File.Delete(LocalTempPath);

                // Move old program to temp path
                File.Move(FilePath, LocalTempPath);
            }
            catch (Exception ex)
            {
                DownloadCancelled(UpdateStage.Install, ex);
                return;
            }

            try
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);

                // Move downloaded update to old program's path
                File.Move(ServerTempPath, FilePath);
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(FilePath))
                        File.Delete(FilePath);

                    // Try to restore local version
                    File.Move(LocalTempPath, FilePath);
                }
                catch (Exception ex2)
                {
                    AddLog($"Exception: {Environment.NewLine}{ex2}", UserLevel.Debug);
                    return;
                }

                DownloadCancelled(UpdateStage.Install, ex);
            }

            // Clear the temporary files
            ClearTemp();

            // Flag that the operation is finished
            OperationRunning = false;

            // Start the updated program
            Process.Start(FilePath, $"-install \"{Assembly.GetExecutingAssembly().Location}\"")?.Dispose();

            // Close the program
            Application.Exit();
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Change the current progress bar value
            progressBar1.Value = e.ProgressPercentage;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the temporary files for this program
        /// </summary>
        private void ClearTemp()
        {
            if (File.Exists(ServerTempPath))
            {
                try
                {
                    File.Delete(ServerTempPath);
                }
                catch (Exception ex)
                {
                    AddLog($"Exception: {Environment.NewLine}{ex}", UserLevel.Debug);
                }
            }

            if (File.Exists(LocalTempPath))
            {
                try
                {
                    File.Delete(LocalTempPath);
                }
                catch (Exception ex)
                {
                    AddLog($"Exception: {Environment.NewLine}{ex}", UserLevel.Debug);
                }
            }
        }

        /// <summary>
        /// Handles a cancellation and closes the program
        /// </summary>
        /// <param name="downloadStage">The stage of the download</param>
        /// <param name="ex">An optional exception causing the cancellation</param>
        private void DownloadCancelled(UpdateStage downloadStage, Exception ex = null)
        {
            // Log the exception if it exists
            if (ex != null)
                AddLog($"Exception: {Environment.NewLine}{ex}", UserLevel.Debug);

            // Clear the temporary files
            ClearTemp();

            // Show error message
            if (downloadStage == UpdateStage.Manifest)
                MessageBox.Show("The loaded manifest is in an incorrect format and can not be read", "Server error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show($"An error occurred while {(downloadStage == UpdateStage.Download ? "downloading" : "installing")} the update", $"{(downloadStage == UpdateStage.Download ? "Download" : "Install")} canceled", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Flag that the operation is not running
            OperationRunning = false;

            // Close the program
            Application.Exit();
        }

        /// <summary>
        /// Adds a local log to the program
        /// </summary>
        /// <param name="text">The log text</param>
        /// <param name="ul">The required user level</param>
        private void AddLog(string text, UserLevel ul = UserLevel.Normal)
        {
            if (CurrentUserLevel >= ul)
                richTextBox1.AppendText(text + Environment.NewLine);
        }

        #endregion
    }
}
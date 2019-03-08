using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the downloader
    /// </summary>
    public class DownloaderViewModel : BaseViewModel
    {
        #region Constructors

        /// <summary>
        /// Downloads a list of files to a specified output directory
        /// </summary>
        /// <param name="inputSources">The files to download</param>
        /// <param name="outputDirectory">The output directory to download to</param>
        public DownloaderViewModel(IEnumerable<Uri> inputSources, FileSystemPath outputDirectory)
        {
            // Get properties
            InputSources = new List<Uri>(inputSources);
            OutputDirectory = outputDirectory;
            IsCompressed = false;
            ProcessedFiles = new List<FileSystemPath>();

            // Set properties
            DownloadState = DownloadState.Paused;
            TotalMaxProgress = InputSources.Count * 100;

            RCF.Logger.LogInformationSource($"A download operation has started with the files {InputSources.JoinItems(", ")}");
        }

        /// <summary>
        /// Downloads a compressed file and extracts its files to a specified output directory
        /// </summary>
        /// <param name="compressedInputSource">The compressed file to download</param>
        /// <param name="outputDirectory">The output directory to extract to</param>
        public DownloaderViewModel(Uri compressedInputSource, FileSystemPath outputDirectory)
        {
            // Get properties
            InputSources = new List<Uri>
            {
                compressedInputSource
            };

            OutputDirectory = outputDirectory;
            IsCompressed = true;
            ProcessedFiles = new List<FileSystemPath>();

            // Set properties
            DownloadState = DownloadState.Paused;
            TotalMaxProgress = 100 + 10;

            RCF.Logger.LogInformationSource($"A download operation has started with the compressed file {compressedInputSource}");
        }

        #endregion

        #region Event Handlers

        private void WCServer_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                ItemMaxProgress = e.TotalBytesToReceive;
                ItemCurrentProgress = e.BytesReceived;

                TotalCurrentProgress = CurrentStep * 100 + e.ProgressPercentage;

                OnStatusUpdated(new Progress(TotalCurrentProgress, TotalMaxProgress));
            }
            catch (Exception ex)
            {
                ex.HandleError("Updating server download progress", e);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Restores a canceled or crashed download
        /// </summary>
        protected async Task RestoreStoppedDownloadAsync()
        {
            // Restore and clean up failed download
            try
            {
                // Delete the downloaded processed files
                foreach (FileSystemPath file in ProcessedFiles)
                {
                    try
                    {
                        RCFRCP.File.DeleteFile(file);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Deleting processed file from incomplete download operation");
                    }
                }

                // Restore backed up files
                foreach (FileSystemPath item in Directory.EnumerateFiles(TempDirLocal, "*", SearchOption.AllDirectories))
                {
                    // Get the path to move to
                    FileSystemPath file = OutputDirectory + item.GetRelativePath(TempDirLocal);

                    try
                    {
                        // Move back the file
                        RCFRCP.File.MoveFile(item, file, true);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Restoring backed up file from incomplete download operation");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Cleaning up incomplete download operation");

                // Let the user know the restore failed
                await RCF.MessageUI.DisplayMessageAsync($"Some files could not be restored. Check {TempDirLocal} & {TempDirServer} to recover lost files.", "Restore and clean up failed", MessageType.Error);
            }
        }

        /// <summary>
        /// Processes the downloaded compressed file
        /// </summary>
        /// <returns>The task</returns>
        protected async Task ProcessCompressedDownloadAsync()
        {
            // Reset file progress
            ItemCurrentProgress = 0;

            // Get the absolute output path
            FileSystemPath file = TempDirServer + Path.GetFileName(InputSources.First().AbsolutePath);

            await Task.Run(async () =>
            {
                // Open the zip file
                using (var zip = ZipFile.OpenRead(file))
                {
                    // Set file progress to its entry count
                    ItemMaxProgress = zip.Entries.Count;

                    // Extract each entry
                    foreach (var entry in zip.Entries)
                    {
                        ThrowIfCancellationRequested();

                        // Get the absolute output path
                        var outputPath = OutputDirectory + entry.FullName.Replace('/', '\\');

                        // Check if the entry is a directory
                        if (entry.FullName.EndsWith("\\") && entry.Name == String.Empty)
                        {
                            // Create directory if it doesn't exist
                            if (!outputPath.DirectoryExists)
                                Directory.CreateDirectory(outputPath);

                            continue;
                        }

                        // Backup conflict file
                        if (outputPath.FileExists)
                            await Task.Run(() => RCFRCP.File.MoveFile(outputPath, TempDirLocal + entry.FullName.Replace('/', '\\'), false));

                        // Create directory if it doesn't exist
                        if (!outputPath.Parent.DirectoryExists)
                            Directory.CreateDirectory(outputPath.Parent);

                        // Extract the compressed file
                        entry.ExtractToFile(outputPath);

                        // Flag the file as processed
                        ProcessedFiles.Add(outputPath);

                        // Increase file progress
                        ItemCurrentProgress++;

                        // Set total progress
                        TotalCurrentProgress = (int)Math.Floor(100 + ((ItemCurrentProgress / ItemMaxProgress) * 10));
                        OnStatusUpdated(new Progress(TotalCurrentProgress, TotalMaxProgress));
                    }
                }
            });

            // Delete the zip file
            RCFRCP.File.DeleteFile(file);
        }

        /// <summary>
        /// Processes the downloaded files
        /// </summary>
        /// <returns>The task</returns>
        protected async Task ProcessDownloadedFilesAsync()
        {
            // Back up existing file
            foreach (var item in InputSources)
            {
                ThrowIfCancellationRequested();

                // Get the absolute output path
                FileSystemPath file = OutputDirectory + Path.GetFileName(item.AbsolutePath);

                // Check if it's a file conflict
                if (!file.FileExists)
                    continue;

                // Move the file to temp
                await Task.Run(() => RCFRCP.File.MoveFile(file, TempDirLocal + file.Name, false));
            }

            // Move downloaded files to output
            foreach (var item in InputSources)
            {
                ThrowIfCancellationRequested();

                // Get the absolute path
                var file = TempDirServer + Path.GetFileName(item.AbsolutePath);

                var outputPath = OutputDirectory + Path.GetFileName(item.AbsolutePath);

                // Move the downloaded file from temp to the output
                RCFRCP.File.MoveFile(file, outputPath, false);

                // Flag the file as processed
                ProcessedFiles.Add(outputPath);
            }
        }

        /// <summary>
        /// Downloads the files specified in <see cref="InputSources"/> to temp
        /// </summary>
        /// <returns>The task</returns>
        protected async Task DownloadFromServerToTempAsync()
        {
            ItemMaxProgress = 100;

            // Create the server web client
            using (WCServer = new WebClient())
            {
                WCServer.DownloadProgressChanged += WCServer_DownloadProgressChanged;

                // Download files
                foreach (var item in InputSources)
                {
                    RCF.Logger.LogInformationSource($"The file {item} is being downloaded");

                    await WCServer.DownloadFileTaskAsync(item, TempDirServer + Path.GetFileName(item.AbsolutePath));
                    ItemCurrentProgress = 100;
                    CurrentStep++;
                }

                WCServer.DownloadProgressChanged -= WCServer_DownloadProgressChanged;
            }
        }

        /// <summary>
        /// Throws a <see cref="OperationCanceledException"/> if cancellation has been requested
        /// </summary>
        protected void ThrowIfCancellationRequested()
        {
            if (CancellationRequested)
                throw new OperationCanceledException();
        }

        /// <summary>
        /// Cleans the temp files
        /// </summary>
        protected void CleanTemp()
        {
            RCFRCP.File.DeleteDirectory(TempDirLocal);
            RCFRCP.File.DeleteDirectory(TempDirServer);
        }

        /// <summary>
        /// Invokes the <see cref="StatusUpdated"/> event
        /// </summary>
        /// <param name="e">The event args</param>
        protected void OnStatusUpdated(Progress e)
        {
            StatusUpdated?.Invoke(this, new ValueEventArgs<Progress>(e));
        }

        /// <summary>
        /// Invokes the <see cref="DownloadComplete"/> event
        /// </summary>
        protected void OnDownloadComplete()
        {
            DownloadComplete?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempts to cancel the installation
        /// </summary>
        public async Task AttemptCancelAsync()
        {
            if (CancellationRequested)
            {
                await RCF.MessageUI.DisplayMessageAsync("The operation is currently canceling", "Cancel request already received", MessageType.Information);
                return;
            }

            if (await RCF.MessageUI.DisplayMessageAsync("Do you wish to cancel the download?", "Cancel ongoing download", MessageType.Question, true))
            {
                RCF.Logger.LogInformationSource($"The downloader has been requested to cancel");
                WCServer?.CancelAsync();
                CancellationRequested = true;
            }
        }

        /// <summary>
        /// Start the download
        /// </summary>
        /// <returns>The task</returns>
        public async Task StartAsync()
        {
            if (OperationRunning)
                throw new InvalidOperationException("The downloader can not start while running");

            DownloadState = DownloadState.Running;

            // Flag the operation as running
            OperationRunning = true;

            // Reset the progress counter
            CurrentStep = 0;

            try
            {
                // Clean existing download temp
                CleanTemp();

                // Create the temp directories
                Directory.CreateDirectory(TempDirLocal);
                Directory.CreateDirectory(TempDirServer);

                // Download files to temp
                await DownloadFromServerToTempAsync();

                // Process the download
                if (IsCompressed)
                    await ProcessCompressedDownloadAsync();
                else
                    await ProcessDownloadedFilesAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Downloading files");

                if (CancellationRequested)
                    await RCF.MessageUI.DisplayMessageAsync("The download operation was canceled", "Operation canceled", MessageType.Information);
                else
                    await RCF.MessageUI.DisplayMessageAsync("The download operation failed", "Operation failed", MessageType.Error);

                // Restore the stopped download
                await RestoreStoppedDownloadAsync();

                // Flag that the operation is no longer running
                OperationRunning = false;
                DownloadState = CancellationRequested ? DownloadState.Canceled : DownloadState.Failed;
                OnDownloadComplete();
                return;
            }

            try
            {
                // Clean temp
                CleanTemp();
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Cleaning download temp");
            }

            // Max out the progress
            ItemCurrentProgress = ItemMaxProgress;
            TotalCurrentProgress = TotalMaxProgress;
            OnStatusUpdated(new Progress(TotalCurrentProgress, TotalMaxProgress));

            // Flag the operation as complete
            OperationRunning = false;

            DownloadState = DownloadState.Succeeded;

            RCF.Logger.LogInformationSource($"The download operation has completed");

            await RCF.MessageUI.DisplayMessageAsync("The files were downloaded successfully", "Download operation complete", MessageType.Success);
            OnDownloadComplete();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when the download status is updated
        /// </summary>
        public event EventHandler<ValueEventArgs<Progress>> StatusUpdated;

        /// <summary>
        /// Occurs when the download is complete or canceled
        /// </summary>
        public event EventHandler DownloadComplete;

        #endregion

        #region Protected Properties

        /// <summary>
        /// The current step in the operation
        /// </summary>
        protected int CurrentStep { get; set; }

        /// <summary>
        /// The web client for server files
        /// </summary>
        protected WebClient WCServer { get; set; }

        /// <summary>
        /// The input sources. This collection only has one item if <see cref="IsCompressed"/> is true
        /// </summary>
        protected List<Uri> InputSources { get; }

        /// <summary>
        /// The output directory
        /// </summary>
        protected FileSystemPath OutputDirectory { get; }

        /// <summary>
        /// The list of processed files
        /// </summary>
        protected List<FileSystemPath> ProcessedFiles { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current total progress
        /// </summary>
        public double TotalCurrentProgress { get; set; }

        /// <summary>
        /// The max total progress
        /// </summary>
        public double TotalMaxProgress { get; set; }

        /// <summary>
        /// The current item progress
        /// </summary>
        public double ItemCurrentProgress { get; set; }

        /// <summary>
        /// The max item progress
        /// </summary>
        public double ItemMaxProgress { get; set; }

        /// <summary>
        /// The current state of the download
        /// </summary>
        public DownloadState DownloadState { get; private set; }

        /// <summary>
        /// Indicates if the operation is running
        /// </summary>
        public bool OperationRunning { get; set; }

        /// <summary>
        /// Indicates if a cancellation has been requested
        /// </summary>
        public bool CancellationRequested { get; set; }

        /// <summary>
        /// Indicates if the download is compressed
        /// </summary>
        public bool IsCompressed { get; }

        #endregion

        #region Protected Static Properties

        /// <summary>
        /// The temp directory for the local files
        /// </summary>
        protected static FileSystemPath TempDirLocal => CommonPaths.TempPath + "Download" + "Local";

        /// <summary>
        /// The temp directory for the sever files
        /// </summary>
        protected static FileSystemPath TempDirServer => CommonPaths.TempPath + "Download" + "Server";

        #endregion
    }
}
using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Handles game installations
    /// </summary>
    public class RayGameInstaller : IStatusUpdated
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="installerData">The data for this installation</param>
        public RayGameInstaller(RayGameInstallerData installerData)
        {
            InstallData = installerData;
        }

        #endregion

        #region Private Fields

        private int _totalItems;

        #endregion

        #region Protected Properties

        /// <summary>
        /// The data for this installation
        /// </summary>
        protected RayGameInstallerData InstallData { get; }

        /// <summary>
        /// The current progress step
        /// </summary>
        protected int CurrentItem { get; set; }

        /// <summary>
        /// The total amount of items
        /// </summary>
        protected int TotalItems
        {
            get => _totalItems;
            set
            {
                _totalItems = value;
                TotalPercentage = TotalItems * 100;
            }
        }

        /// <summary>
        /// The total progress percentage
        /// </summary>
        protected int TotalPercentage { get; set; } = 100;

        /// <summary>
        /// The currently processed object
        /// </summary>
        protected object CurrentObject { get; set; }

        /// <summary>
        /// The drives used for this installation
        /// </summary>
        protected RayGameDriveInfo[] Drives { get; set; }

        /// <summary>
        /// True if the installation has been verified and is ready to run
        /// </summary>
        protected bool IsVerified => Drives != null;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the status of the installation is updated
        /// </summary>
        public event StatusUpdateEventHandler StatusUpdated;

        #endregion

        #region Private Methods

        /// <summary>
        /// Calls the <see cref="StatusUpdated"/> event
        /// </summary>
        /// <param name="operationState">The state of the operation</param>
        /// <param name="itemProgress">The item progress</param>
        private void OnStatusUpdated(OperationState operationState = OperationState.Running, Progress? itemProgress = null)
        {
            OnStatusUpdated(new OperationProgressEventArgs(new ItemsOperationProgress()
            {
                // Set the name to the install method
                OperationName = nameof(InstallAsync),

                // Set the item progress
                ItemProgress = itemProgress ?? new Progress(0),

                // Set the total progress
                TotalProgress = new Progress((CurrentItem * 100) + (itemProgress?.Percentage ?? 0), TotalPercentage),

                // Set the current item
                CurrentItem = CurrentObject
            }, operationState));
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Calls the <see cref="StatusUpdated"/> event
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected void OnStatusUpdated(OperationProgressEventArgs e)
        {
            StatusUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Verifies the installation
        /// </summary>
        /// <returns>True if the verification succeeded, false if it was canceled</returns>
        protected virtual async Task<bool> VerifyInstallationAsync()
        {
            // Make a list of the drives used
            List<RayGameDriveInfo> drives = new List<RayGameDriveInfo>();

            // Keep track of when the verification is complete
            bool verifyComplete = false;

            // Get the base paths
            while (!verifyComplete)
            {
                // Check if cancellation has been requested
                InstallData.CancellationToken.ThrowIfCancellationRequested();

                // Update the status to paused
                OnStatusUpdated(OperationState.Paused);

                // Get a drive from the user
                var result = await RCFUI.BrowseUI.BrowseDriveAsync(new DriveBrowserViewModel()
                {
                    Title = Resources.Installer_BrowseDiscHeader,
                    MultiSelection = false,
                    AllowNonReadyDrives = false,
                    DefaultDirectory = @"D:\"
                });

                // Make sure the user didn't cancel
                if (result.CanceledByUser)
                    return false;

                // Update the status to default
                OnStatusUpdated();

                // Get the selected drive
                FileSystemPath drive = result.SelectedDrive;

                // Keep track if any new items are added
                bool anyAdded = false;

                // Keep track of missing items
                int missingItems = 0;

                // Run as a task to avoid locking UI thread
                await Task.Run(() =>
                {
                    // Check the remaining items if they exist on the drive
                    foreach (RayGameInstallItem item in InstallData.RelativeInputs.Where(x => x.ProcessStage == RayGameInstallItemStage.Initial))
                    {
                        // Attempt to add drive to item
                        if (item.AddIfExists(drive))
                        {
                            // Flag that the item has been verified
                            item.ProcessStage = RayGameInstallItemStage.Verified;
                            anyAdded = true;
                        }
                        else if (!item.Optional)
                        {
                            // Add to list of missing items
                            missingItems++;
                        }

                        // Check if cancellation has been requested
                        InstallData.CancellationToken.ThrowIfCancellationRequested();
                    }
                });

                // Save drive information if new items were added
                if (anyAdded)
                {
                    RCFCore.Logger?.LogInformationSource($"The drive {drive} was added to the installation");

                    // Get the drive info
                    var driveInfo = new RayGameDriveInfo(drive, new DriveInfo(drive).VolumeLabel);

                    // Make sure the label and drive path are not the same as an existing one
                    if (drives.Any(x => x.Root == driveInfo.Root && x.VolumeLabel == driveInfo.VolumeLabel))
                    {
                        await RCFUI.MessageUI.DisplayMessageAsync(Resources.Installer_DriveNameConflict, Resources.Installer_DriveNameConflictHeader, MessageType.Error);
                        return false;
                    }

                    // Add the drive info
                    drives.Add(driveInfo);
                }
                else
                {
                    RCFCore.Logger?.LogInformationSource($"The drive {drive} was not added to the installation");
                }

                // Check if only optional items are remaining
                if (InstallData.RelativeInputs.All(x => x.Optional || x.ProcessStage == RayGameInstallItemStage.Verified))
                    verifyComplete = true;

                // Display message to user if there are items remaining
                if (!verifyComplete)
                {
                    // Update the status to paused
                    OnStatusUpdated(OperationState.Paused);

                    await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_MissingFiles, missingItems), Resources.Installer_MissingFilesHeader, MessageType.Information);

                    // Update the status to default
                    OnStatusUpdated();
                }
            }

            // Save the drives
            Drives = drives.ToArray();

            RCFCore.Logger?.LogInformationSource($"The drives have been verified as {Drives.JoinItems(", ")}");

            return true;
        }

        /// <summary>
        /// Requests the specified drive to be available
        /// </summary>
        /// <param name="drive">The drive to request</param>
        /// <returns>True if the drive is available, false if the request was canceled</returns>
        protected virtual async Task<bool> RequestDriveAsync(RayGameDriveInfo drive)
        {
            RCFCore.Logger?.LogInformationSource($"The drive {drive.Root} has been requested");

            // Make sure the drive is available
            while (!drive.IsAvailable)
            {
                // Check if cancellation has been requested
                InstallData.CancellationToken.ThrowIfCancellationRequested();

                // Update the status to paused
                OnStatusUpdated(OperationState.Paused);

                if (!await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_InsertDriveRequest, drive.VolumeLabel, drive.Root), Resources.Installer_InsertDriveRequestHeader, MessageType.Information, true))
                    return false;

                // Update the status to default
                OnStatusUpdated();
            }

            RCFCore.Logger?.LogInformationSource($"The drive {drive.Root} is available");

            return true;
        }

        /// <summary>
        /// Handles an item during the installation
        /// </summary>
        /// <param name="wc">The web client to use</param>
        /// <param name="item">The item</param>
        /// <returns>The task</returns>
        protected virtual async Task HandleItemAsync(WebClient wc, RayGameInstallItem item)
        {
            RCFCore.Logger?.LogDebugSource($"The installation item {item.BasePath} is being handled");

            // Check if cancellation has been requested
            InstallData.CancellationToken.ThrowIfCancellationRequested();

            // Set the currently processed object
            CurrentObject = item;

            // Update the status
            OnStatusUpdated();

            if (item.InputPath.DirectoryExists)
            {
                // Create the directory
                Directory.CreateDirectory(item.OutputPath);

                // Flag that the item has been handled
                item.ProcessStage = RayGameInstallItemStage.Complete;

                RCFCore.Logger?.LogDebugSource($"The installation item {item.BasePath} has been handled as a directory");
            }
            else if (item.InputPath.FileExists)
            {
                // Create parent directory if it doesn't exist
                Directory.CreateDirectory(item.OutputPath.Parent);

                // Copy the file
                await CopyFileAsync(item.InputPath, item.OutputPath, wc);

                // Flag that the item has been handled
                item.ProcessStage = RayGameInstallItemStage.Complete;

                RCFCore.Logger?.LogDebugSource($"The installation item {item.BasePath} has been handled as a file");
            }
            else
            {
                RCFCore.Logger?.LogWarningSource($"The installation item {item.BasePath} is not a valid file or directory");
            }

            CurrentItem++;

            // Update the status
            OnStatusUpdated(itemProgress: new Progress(1, 1));
        }

        /// <summary>
        /// Copies the specified file during the installation
        /// </summary>
        /// <param name="source">The source file to copy</param>
        /// <param name="destination">The destination to copy to</param>
        /// <param name="wc">The web client to use</param>
        /// <returns>The task</returns>
        protected virtual async Task CopyFileAsync(FileSystemPath source, FileSystemPath destination, WebClient wc)
        {
            bool retry = true;

            while (retry)
            {
                try
                {
                    await wc.DownloadFileTaskAsync(source, destination);
                    retry = false;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Copying file");

                    // Throw if cancellation has been requested
                    if (ex is WebException we && we.Status == WebExceptionStatus.RequestCanceled)
                        throw;

                    RCFCore.Logger?.LogInformationSource($"Failed to copy file {source.FullPath} during installation. Requesting retry.");

                    // Ask user to retry
                    if (!await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_FileCopyError, source.Name, ex.Message), Resources.Installer_FileCopyErrorHeader, MessageType.Warning, true))
                        throw;

                    RCFCore.Logger?.LogInformationSource($"Attempting to retry to copy file");

                    // Remove partially copied file
                    RCFRCP.File.DeleteFile(destination);

                    // Allow retry
                    retry = true;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Runs the installation
        /// </summary>
        /// <returns>The task to run</returns>
        public virtual async Task<RayGameInstallerResult> InstallAsync()
        {
            RCFCore.Logger?.LogInformationSource($"An installation has begun");

            // Store the web client
            WebClient wc = null;

            using (var tempDir = new TempDirectory())
            {
                // Flag indicating if we've got file conflicts
                bool fileConflicts = false;

                // Flag indicating if the installation was completed
                bool complete = false;

                try
                {
                    // Register the cancellation token callback to cancel the web client's ongoing operation
                    InstallData.CancellationToken.Register(() => wc?.CancelAsync());

                    // Check if cancellation has been requested
                    InstallData.CancellationToken.ThrowIfCancellationRequested();

                    // Verify the installation
                    if (!await VerifyInstallationAsync())
                        return RayGameInstallerResult.Canceled;

                    RCFCore.Logger?.LogInformationSource($"The installation has been verified");

                    // Check if the output directory already exists
                    if (InstallData.OutputDir.DirectoryExists)
                    {
                        RCFCore.Logger?.LogInformationSource($"The installation output already exists");

                        var existingPaths = new List<FileSystemPath>();

                        // Check for any file conflicts
                        foreach (RayGameInstallItem source in InstallData.RelativeInputs)
                            if (source.OutputPath.FileExists)
                                existingPaths.Add(source.OutputPath);

                        // Check if we got any existing paths
                        if (existingPaths.Count > 0)
                        {
                            RCFCore.Logger?.LogInformationSource($"The installation output has file conflicts");

                            // Update the status to paused
                            OnStatusUpdated(OperationState.Paused);

                            // Flag that we have file conflicts
                            fileConflicts = true;

                            if (!(await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_FileConflicts, existingPaths.Count), Resources.Installer_FileConflictsHeader, MessageType.Question, true)))
                                return RayGameInstallerResult.Canceled;

                            // Update the status to default
                            OnStatusUpdated();

                            // Move file conflicts to temp
                            foreach (FileSystemPath path in existingPaths)
                                // Move file
                                RCFRCP.File.MoveFile(path, tempDir.TempPath + path.GetRelativePath(InstallData.OutputDir), false);
                        }
                    }

                    // Create the web client
                    wc = new WebClient();

                    // Subscribe to when the progress changes
                    wc.DownloadProgressChanged += (s, e) => OnStatusUpdated(itemProgress: new Progress(e.BytesReceived, e.TotalBytesToReceive));

                    // Set the item counts
                    TotalItems = InstallData.RelativeInputs.Count;
                    CurrentItem = 0;

                    // Copy files and directories from the saved drives
                    foreach (RayGameDriveInfo drive in Drives)
                    {
                        // Request the drive
                        if (!await RequestDriveAsync(drive))
                            return RayGameInstallerResult.Canceled;

                        // Copy each file and directory from the current drive
                        foreach (var item in InstallData.RelativeInputs.Where(x => x.BaseDriveLabel == drive.VolumeLabel && x.BasePath == drive.Root && x.ProcessStage == RayGameInstallItemStage.Verified))
                            await HandleItemAsync(wc, item);
                    }

                    // Make sure no items were not handled
                    while (InstallData.RelativeInputs.Any(x => x.ProcessStage == RayGameInstallItemStage.Verified))
                    {
                        // Update the status to paused
                        OnStatusUpdated(OperationState.Paused);

                        if (!(await RCFUI.MessageUI.DisplayMessageAsync(Resources.Installer_UnhandledItems, Resources.Installer_UnhandledItemsHeader, MessageType.Warning, true)))
                            return RayGameInstallerResult.Canceled;

                        // Update the status to default
                        OnStatusUpdated();

                        // Check each drive
                        foreach (RayGameDriveInfo drive in Drives)
                        {
                            // Get the item for this drive
                            var items = InstallData.RelativeInputs.Where(x => x.ProcessStage == RayGameInstallItemStage.Verified && x.BaseDriveLabel == drive.VolumeLabel && x.BasePath == drive.Root).ToList();

                            // Skip if there are no items
                            if (!items.Any())
                                continue;

                            // Request the drive
                            if (!await RequestDriveAsync(drive))
                                return RayGameInstallerResult.Canceled;

                            // Copy each file and directory from the current drive
                            foreach (var item in items)
                                await HandleItemAsync(wc, item);
                        }
                    }

                    // Flag that the installation completed
                    complete = true;

                    RCFCore.Logger?.LogInformationSource($"The installation has completed");

                    return RayGameInstallerResult.Successful;
                }
                catch (Exception ex)
                {
                    if (InstallData.CancellationToken.IsCancellationRequested)
                    {
                        ex.HandleExpected("Installing game");
                        return RayGameInstallerResult.Canceled;
                    }
                    else
                    {
                        ex.HandleError("Installing game", InstallData);
                        return RayGameInstallerResult.Failed;
                    }
                }
                finally
                {
                    // Dispose the web client
                    wc?.Dispose();

                    // Clean up if not complete
                    if (!complete)
                    {
                        OnStatusUpdated(OperationState.Error);

                        // Keep track of errors
                        bool error = false;

                        // Delete incomplete installation files
                        foreach (var item in InstallData.RelativeInputs.Where(x => x.ProcessStage == RayGameInstallItemStage.Complete && x.OutputPath.FileExists))
                        {
                            try
                            {
                                // Delete the file
                                RCFRCP.File.DeleteFile(item.OutputPath);
                            }
                            catch (Exception ex2)
                            {
                                ex2.HandleError("Deleting incomplete installation item", item);
                                error = true;
                            }
                        }

                        if (error && RCFCore.Data.CurrentUserLevel >= UserLevel.Advanced)
                            await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_CleanupError, InstallData.OutputDir), MessageType.Error);

                        try
                        {
                            // Delete output if empty
                            if (!Directory.EnumerateFiles(InstallData.OutputDir, "", SearchOption.AllDirectories).Any())
                                RCFRCP.File.DeleteDirectory(InstallData.OutputDir);
                        }
                        catch (Exception ex2)
                        {
                            ex2.HandleError("Attempting to delete incomplete installation output", InstallData);
                        }

                        // Restore from temp if there were any file conflicts
                        if (fileConflicts)
                        {
                            try
                            {
                                RCFRCP.File.CopyDirectory(tempDir.TempPath, InstallData.OutputDir, false, true);
                            }
                            catch (Exception ex2)
                            {
                                ex2.HandleError("Restoring files from incomplete installation", InstallData);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
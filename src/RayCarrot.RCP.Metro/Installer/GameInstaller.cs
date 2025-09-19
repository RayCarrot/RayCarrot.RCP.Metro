using System.IO;
using System.Net;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Handles game installations
/// </summary>
public class GameInstaller : IStatusUpdated, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="installerData">The data for this installation</param>
    public GameInstaller(GameInstaller_Data installerData)
    {
        WebClient = new WebClient();
        InstallData = installerData;
        FileManager = Services.File;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private int _totalItems;

    #endregion

    #region Protected Properties

    /// <summary>
    /// The file manager
    /// </summary>
    protected FileManager FileManager { get; }

    /// <summary>
    /// The data for this installation
    /// </summary>
    protected GameInstaller_Data InstallData { get; }

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
    protected GameInstaller_DriveInfo[] Drives { get; set; }

    /// <summary>
    /// True if the installation has been verified and is ready to run
    /// </summary>
    protected bool IsVerified => Drives != null;

    /// <summary>
    /// The web client used to copy the files
    /// </summary>
    protected WebClient WebClient { get; }

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
            TotalProgress = new Progress((CurrentItem * 100) + (itemProgress?.Percentage_100 ?? 0), TotalPercentage),

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
        List<GameInstaller_DriveInfo> drives = new List<GameInstaller_DriveInfo>();

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
            var result = await Services.UI.BrowseDriveAsync(new DriveBrowserViewModel()
            {
                Title = Resources.Installer_BrowseDiscHeader,
                MultiSelection = false,
                AllowNonReadyDrives = false,
                DefaultDirectory = @"D:\",
                AllowedTypes = new DriveType[]
                {
                    DriveType.CDRom
                }
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
                foreach (GameInstaller_Item item in InstallData.RelativeInputs.Where(x => x.ProcessStage == GameInstaller_ItemStage.Initial))
                {
                    // Attempt to add drive to item
                    if (item.AddIfExists(drive))
                    {
                        // Flag that the item has been verified
                        item.ProcessStage = GameInstaller_ItemStage.Verified;
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
                Logger.Info("The drive {0} was added to the installation", drive);

                // Get the drive info
                var driveInfo = new GameInstaller_DriveInfo(drive, new DriveInfo(drive).VolumeLabel);

                // Make sure the label and drive path are not the same as an existing one
                if (drives.Any(x => x.Root == driveInfo.Root && x.VolumeLabel == driveInfo.VolumeLabel))
                {
                    await Services.MessageUI.DisplayMessageAsync(Resources.Installer_DriveNameConflict, Resources.Installer_DriveNameConflictHeader, MessageType.Error);
                    return false;
                }

                // Add the drive info
                drives.Add(driveInfo);
            }
            else
            {
                Logger.Info("The drive {0} was not added to the installation", drive);
            }

            // Check if only optional items are remaining
            if (InstallData.RelativeInputs.All(x => x.Optional || x.ProcessStage == GameInstaller_ItemStage.Verified))
                verifyComplete = true;

            // Display message to user if there are items remaining
            if (!verifyComplete)
            {
                // Update the status to paused
                OnStatusUpdated(OperationState.Paused);

                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_MissingFiles, missingItems), Resources.Installer_MissingFilesHeader, MessageType.Information);

                // Update the status to default
                OnStatusUpdated();
            }
        }

        // Save the drives
        Drives = drives.ToArray();

        Logger.Info("The drives have been verified as {0}", Drives.JoinItems(", "));

        return true;
    }

    /// <summary>
    /// Requests the specified drive to be available
    /// </summary>
    /// <param name="drive">The drive to request</param>
    /// <returns>True if the drive is available, false if the request was canceled</returns>
    protected virtual async Task<bool> RequestDriveAsync(GameInstaller_DriveInfo drive)
    {
        Logger.Info("The drive {0} has been requested", drive.Root);

        // Make sure the drive is available
        while (!drive.IsAvailable)
        {
            // Check if cancellation has been requested
            InstallData.CancellationToken.ThrowIfCancellationRequested();

            // Update the status to paused
            OnStatusUpdated(OperationState.Paused);

            if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_InsertDriveRequest, drive.VolumeLabel, drive.Root), Resources.Installer_InsertDriveRequestHeader, MessageType.Information, true))
                return false;

            // Update the status to default
            OnStatusUpdated();
        }

        Logger.Info("The drive {0} is available", drive.Root);

        return true;
    }

    /// <summary>
    /// Handles an item during the installation
    /// </summary>
    /// <param name="wc">The web client to use</param>
    /// <param name="item">The item</param>
    /// <returns>The task</returns>
    protected virtual async Task HandleItemAsync(WebClient wc, GameInstaller_Item item)
    {
        Logger.Debug("The installation item {0} is being handled", item.BasePath);

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
            item.ProcessStage = GameInstaller_ItemStage.Complete;

            Logger.Debug("The installation item {0} has been handled as a directory", item.BasePath);
        }
        else if (item.InputPath.FileExists)
        {
            // Create parent directory if it doesn't exist
            Directory.CreateDirectory(item.OutputPath.Parent);

            // Copy the file
            await CopyFileAsync(item.InputPath, item.OutputPath, wc);

            // Flag that the item has been handled
            item.ProcessStage = GameInstaller_ItemStage.Complete;

            Logger.Debug("The installation item {0} has been handled as a file", item.BasePath);
        }
        else
        {
            Logger.Warn("The installation item {0} is not a valid file or directory", item.BasePath);
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
                // Throw if cancellation has been requested
                if (ex is WebException we && we.Status == WebExceptionStatus.RequestCanceled)
                {
                    Logger.Debug(ex, "Copying file");
                    throw;
                }

                Logger.Error(ex, "Copying file");

                Logger.Info("Failed to copy file {0} during installation. Requesting retry.", source.FullPath);

                // Ask user to retry
                if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_FileCopyError, source.Name, ex.Message), Resources.Installer_FileCopyErrorHeader, MessageType.Warning, true))
                    throw;

                Logger.Info("Attempting to retry to copy file");

                // Remove partially copied file
                FileManager.DeleteFile(destination);

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
    public virtual async Task<GameInstaller_Result> InstallAsync()
    {
        Logger.Info("An installation has begun");

        // Flag indicating if the installation was completed
        bool complete = false;

        // Flag indicating if the installation has started
        var installationStarted = false;

        try
        {
            // Register the cancellation token callback to cancel the web client's ongoing operation
            InstallData.CancellationToken.Register(() => WebClient?.CancelAsync());

            // Check if cancellation has been requested
            InstallData.CancellationToken.ThrowIfCancellationRequested();

            // Verify the installation
            if (!await VerifyInstallationAsync())
                return GameInstaller_Result.Canceled;

            Logger.Info("The installation has been verified");

            // Check if the output directory already exists
            if (InstallData.OutputDir.DirectoryExists)
            {
                Logger.Info("The installation output already exists");

                // Update the status to paused
                OnStatusUpdated(OperationState.Paused);

                // Ask user to overwrite files
                if (!(await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_OverwriteOutput, InstallData.OutputDir), Resources.Installer_OverwriteOutputHeader, MessageType.Question, true)))
                    return GameInstaller_Result.Canceled;

                // Delete the existing directory
                FileManager.DeleteDirectory(InstallData.OutputDir);

                // Update the status to default
                OnStatusUpdated();
            }

            // Subscribe to when the progress changes
            WebClient.DownloadProgressChanged += (s, e) => OnStatusUpdated(itemProgress: new Progress(e.BytesReceived, e.TotalBytesToReceive));

            // Set the item counts
            TotalItems = InstallData.RelativeInputs.Count;
            CurrentItem = 0;

            installationStarted = true;

            // Copy files and directories from the saved drives
            foreach (GameInstaller_DriveInfo drive in Drives)
            {
                // Request the drive
                if (!await RequestDriveAsync(drive))
                    return GameInstaller_Result.Canceled;

                // Copy each file and directory from the current drive
                foreach (var item in InstallData.RelativeInputs.Where(x => x.BaseDriveLabel == drive.VolumeLabel && x.BasePath == drive.Root && x.ProcessStage == GameInstaller_ItemStage.Verified))
                    await HandleItemAsync(WebClient, item);
            }

            // Make sure no items were not handled
            while (InstallData.RelativeInputs.Any(x => x.ProcessStage == GameInstaller_ItemStage.Verified))
            {
                // Update the status to paused
                OnStatusUpdated(OperationState.Paused);

                if (!(await Services.MessageUI.DisplayMessageAsync(Resources.Installer_UnhandledItems, Resources.Installer_UnhandledItemsHeader, MessageType.Warning, true)))
                    return GameInstaller_Result.Canceled;

                // Update the status to default
                OnStatusUpdated();

                // Check each drive
                foreach (GameInstaller_DriveInfo drive in Drives)
                {
                    // Get the item for this drive
                    var items = InstallData.RelativeInputs.Where(x => x.ProcessStage == GameInstaller_ItemStage.Verified && x.BaseDriveLabel == drive.VolumeLabel && x.BasePath == drive.Root).ToArray();

                    // Skip if there are no items
                    if (!items.Any())
                        continue;

                    // Request the drive
                    if (!await RequestDriveAsync(drive))
                        return GameInstaller_Result.Canceled;

                    // Copy each file and directory from the current drive
                    foreach (var item in items)
                        await HandleItemAsync(WebClient, item);
                }
            }

            // Flag that the installation completed
            complete = true;

            Logger.Info("The installation has completed");

            return GameInstaller_Result.Successful;
        }
        catch (Exception ex)
        {
            if (InstallData.CancellationToken.IsCancellationRequested)
            {
                Logger.Debug(ex, "Installing game");
                return GameInstaller_Result.Canceled;
            }
            else
            {
                Logger.Error(ex, "Installing game");
                return GameInstaller_Result.Failed;
            }
        }
        finally
        {
            // Clean up if started and not complete
            if (installationStarted && !complete)
            {
                OnStatusUpdated(OperationState.Error);

                try
                {
                    // Delete install directory
                    FileManager.DeleteDirectory(InstallData.OutputDir);
                }
                catch (Exception ex2)
                {
                    Logger.Error(ex2, "Deleting incomplete installation directory");

                    if (Services.InstanceData.CurrentUserLevel >= UserLevel.Advanced)
                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_CleanupError, InstallData.OutputDir), MessageType.Error);
                }
            }
        }
    }

    /// <summary>
    /// Disposes the web client
    /// </summary>
    public void Dispose()
    {
        WebClient?.Dispose();
    }

    #endregion
}
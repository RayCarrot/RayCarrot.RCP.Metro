using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using ByteSizeLib;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Metro;

// TODO: There's a lot that shouldn't be in here, especially the methods. Break this apart into multiple services.

/// <summary>
/// Handles common actions and events for this application
/// </summary>
public class AppViewModel : BaseViewModel
{
    #region Static Constructor

    static AppViewModel()
    {
        // Get the current Window version
        WindowsVersion = WindowsHelpers.GetCurrentWindowsVersion();
    }

    #endregion

    #region Constructor

    public AppViewModel(
        IUpdaterManager updater, 
        IMessageUIManager message,
        FileManager file,
        AppUIManager ui,
        DeployableFilesManager deployableFiles,
        AppUserData data)
    {
        // Set properties
        Updater = updater ?? throw new ArgumentNullException(nameof(updater));
        MessageUI = message ?? throw new ArgumentNullException(nameof(message));
        File = file ?? throw new ArgumentNullException(nameof(file));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        DeployableFiles = deployableFiles ?? throw new ArgumentNullException(nameof(deployableFiles));
        Data = data ?? throw new ArgumentNullException(nameof(data));

        // Check if the application is running as administrator
        try
        {
            IsRunningAsAdmin = WindowsHelpers.RunningAsAdmin;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error");
            IsRunningAsAdmin = false;
        }

        LoaderViewModel = new LoaderViewModel();

        // Create locks
        AdminWorkerAsyncLock = new AsyncLock();

        // Create commands
        RestartAsAdminCommand = new AsyncRelayCommand(RestartAsAdminAsync);
        RequestRestartAsAdminCommand = new AsyncRelayCommand(RequestRestartAsAdminAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand RestartAsAdminCommand { get; }

    public ICommand RequestRestartAsAdminCommand { get; }

    #endregion

    #region Constant Fields

    // TODO: Remove from here
    /// <summary>
    /// The application base path to use for WPF related operations
    /// </summary>
    public const string WPFApplicationBasePath = "pack://application:,,,/RayCarrot.RCP.Metro;component/";

    #endregion

    #region Services

    private IUpdaterManager Updater { get; }
    private IMessageUIManager MessageUI { get; }
    private FileManager File { get; }
    private AppUIManager UI { get; }
    private DeployableFilesManager DeployableFiles { get; }
    private AppUserData Data { get; }

    #endregion

    #region Private Properties

    /// <summary>
    /// An async lock for the <see cref="RunAdminWorkerAsync"/> method
    /// </summary>
    private AsyncLock AdminWorkerAsyncLock { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The current app version
    /// </summary>
    public Version CurrentAppVersion => AppVersion;

    /// <summary>
    /// Indicates if the current version is a beta version
    /// </summary>
    public bool IsBeta => false;

    /// <summary>
    /// A flag indicating if an update check is in progress
    /// </summary>
    public bool CheckingForUpdates { get; set; }

    /// <summary>
    /// Indicates if the application is running as administrator
    /// </summary>
    public bool IsRunningAsAdmin { get; }

    /// <summary>
    /// Indicates if the application startup is running
    /// </summary>
    public bool IsStartupRunning { get; set; }

    /// <summary>
    /// The view model to use when running an async operation which needs to load
    /// </summary>
    public LoaderViewModel LoaderViewModel { get; }

    #endregion

    #region Public Static Properties

    /// <summary>
    /// The Windows version the program is running on
    /// </summary>
    public static WindowsVersion WindowsVersion { get; } // TODO: Why is this static?

    public static Version AppVersion => new(13, 4, 5, 0);

    #endregion

    #region Public Methods

    /// <summary>
    /// Enables write access to the primary ubi.ini file
    /// </summary>
    /// <returns>The task</returns>
    public async Task EnableUbiIniWriteAccessAsync()
    {
        try
        {
            if (!AppFilePaths.UbiIniPath1.FileExists)
            {
                Logger.Info("The ubi.ini file was not found");
                return;
            }

            // Check if we have write access
            if (File.CheckFileWriteAccess(AppFilePaths.UbiIniPath1))
            {
                Logger.Debug("The ubi.ini file has write access");
                return;
            }

            await MessageUI.DisplayMessageAsync(Resources.UbiIniWriteAccess_InfoMessage);

            // Attempt to change the permission
            await RunAdminWorkerAsync(AdminWorkerMode.GrantFullControl, true, AppFilePaths.UbiIniPath1);

            Logger.Info("The ubi.ini file permission was changed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Changing ubi.ini file permissions");
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.UbiIniWriteAccess_Error);
        }
    }

    /// <summary>
    /// Downloads the specified files to a specified output directory
    /// </summary>
    /// <param name="inputSources">The files to download</param>
    /// <param name="isCompressed">True if the download is a compressed file, otherwise false</param>
    /// <param name="outputDir">The output directory to download to</param>
    /// <param name="isGame">Indicates if the download is for a game. If false it is assumed to be a generic patch.</param>
    /// <returns>True if the download succeeded, otherwise false</returns>
    public async Task<bool> DownloadAsync(IList<Uri> inputSources, bool isCompressed, FileSystemPath outputDir, bool isGame = false)
    {
        try
        {
            Logger.Info("A download is starting...");

            // Make sure there are input sources to download
            if (!inputSources.Any())
            {
                Logger.Info("Download failed due to there not being any input sources");

                await MessageUI.DisplayMessageAsync(Resources.Download_NoFilesFound, MessageType.Error);
                return false;
            }

            if (Data.App_HandleDownloadsManually)
            {
                bool result = await UI.DisplayMessageAsync(Resources.Download_ManualInstructions, Resources.Download_ManualHeader, MessageType.Information, true, new DialogMessageActionViewModel[]
                {
                    // Download files
                    new DialogMessageActionViewModel
                    {
                        DisplayText = Resources.Download_ManualDownload,
                        ShouldCloseDialog = false,
                        OnHandled = () =>
                        {
                            foreach (var u in inputSources)
                                OpenUrl(u.AbsoluteUri);
                        }
                    },
                    // Open destination folder
                    new DialogMessageActionViewModel
                    {
                        DisplayText = Resources.Download_ManualOpenDestination,
                        ShouldCloseDialog = false,
                        OnHandled = () =>
                        {
                            Directory.CreateDirectory(outputDir);
                            Task.Run(async () => await File.OpenExplorerLocationAsync(outputDir));
                        }
                    },
                });

                if (!result)
                    return false;

                Logger.Info("Manual download finished");

                // Return the result
                return true;
            }
            else
            {
                // Allow user to confirm
                try
                {
                    ByteSize size = ByteSize.FromBytes(0);
                    foreach (Uri item in inputSources)
                    {
                        WebRequest webRequest = WebRequest.Create(item);
                        webRequest.Method = "HEAD";

                        using WebResponse webResponse = webRequest.GetResponse();
                        size = size.Add(ByteSize.FromBytes(Convert.ToDouble(webResponse.Headers.Get("Content-Length"))));
                    }

                    Logger.Debug("The size of the download has been retrieved as {0}", size);

                    string msg = isGame ? Resources.DownloadGame_ConfirmSize : Resources.Download_ConfirmSize;

                    if (!await MessageUI.DisplayMessageAsync(String.Format(msg, size), Resources.Download_ConfirmHeader, MessageType.Question, true))
                        return false;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Getting download size");
                    if (!await MessageUI.DisplayMessageAsync(isGame ? Resources.DownloadGame_Confirm : Resources.Download_Confirm, Resources.Download_ConfirmHeader, MessageType.Question, true))
                        return false;
                }
            }

            // Show the downloader and get the result
            DownloaderResult dialogResult = await UI.DownloadAsync(new DownloaderViewModel(inputSources, outputDir, isCompressed));

            Logger.Info("The download finished with the result of {0}", dialogResult.DownloadState);

            // Return the result
            return dialogResult.DownloadState == DownloaderViewModel.DownloadState.Succeeded;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Downloading files");
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.Download_Error);
            return false;
        }
    }

    /// <summary>
    /// Restarts the Rayman Control Panel as administrator
    /// </summary>
    /// <returns>The task</returns>
    public async Task RestartAsAdminAsync()
    {
        // Run the admin worker, setting it to restart this process as admin
        await RunAdminWorkerAsync(AdminWorkerMode.RestartAsAdmin, true, Process.GetCurrentProcess().Id.ToString());
    }

    /// <summary>
    /// Restarts the Rayman Control Panel with the specified arguments
    /// </summary>
    /// <returns>The task</returns>
    public async Task RestartAsync(params string[] args)
    {
        // Run the admin worker, setting it to restart this process as admin
        await RunAdminWorkerAsync(AdminWorkerMode.RestartWithArgs, false, new string[]
        {
            Process.GetCurrentProcess().Id.ToString()
        }.Concat(args).ToArray());
    }

    /// <summary>
    /// Runs the admin worker
    /// </summary>
    /// <param name="mode">The mode to run in</param>
    /// <param name="asAdmin">Indicates if the admin worker should be run with admin privileges</param>
    /// <param name="args">The mode arguments</param>
    /// <returns>The task</returns>
    public async Task RunAdminWorkerAsync(AdminWorkerMode mode, bool asAdmin, params string[] args)
    {
        // Lock
        using (await AdminWorkerAsyncLock.LockAsync())
        {
            FileSystemPath filePath;

            try
            {
                filePath = DeployableFiles.DeployFile(DeployableFilesManager.DeployableFile.AdminWorker);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Deploying admin worker");
                await MessageUI.DisplayExceptionMessageAsync(ex, Resources.DeployFilesError);
                return;
            }

            // Launch the admin worker with the specified launch arguments
            await File.LaunchFileAsync(filePath, asAdmin, $"{mode} {args.Select(x => $"\"{x}\"").JoinItems(" ")}");
        }
    }

    /// <summary>
    /// Requests the application to restart as administrator
    /// </summary>
    /// <returns>The task</returns>
    public async Task RequestRestartAsAdminAsync()
    {
        // Request restarting the application as admin
        if (await MessageUI.DisplayMessageAsync(Resources.App_RequiresAdminQuestion, Resources.App_RestartAsAdmin, MessageType.Warning, true))
            // Restart as admin
            await RestartAsAdminAsync();
    }

    /// <summary>
    /// Opens the specified URL
    /// </summary>
    /// <param name="url">The URL to open</param>
    public void OpenUrl(string url)
    {
        try
        {
            // Open the URL in default browser
            Process.Start(url)?.Dispose();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Opening URL {0}", url);
        }
    }

    /// <summary>
    /// Checks for application updates
    /// </summary>
    /// <param name="isManualSearch">Indicates if this is a manual check, in which cause a message should be shown if no update is found</param>
    /// <returns>The task</returns>
    public async Task CheckForUpdatesAsync(bool isManualSearch)
    {
        if (CheckingForUpdates)
            return;

        try
        {
            CheckingForUpdates = true;

            // Check for updates
            UpdaterCheckResult result = await Updater.CheckAsync(Data.Update_ForceUpdate && isManualSearch, Data.Update_GetBetaUpdates || IsBeta);

            // Check if there is an error
            if (result.ErrorMessage != null)
            {
                await MessageUI.DisplayExceptionMessageAsync(result.Exception,
                    String.Format(Resources.Update_CheckFailed, result.ErrorMessage, AppURLs.RCPBaseUrl), Resources.Update_ErrorHeader);

                Data.Update_IsUpdateAvailable = false;

                return;
            }

            // Check if no new updates were found
            if (!result.IsNewUpdateAvailable)
            {
                if (isManualSearch)
                    await MessageUI.DisplayMessageAsync(String.Format(Resources.Update_LatestInstalled, CurrentAppVersion), Resources.Update_LatestInstalledHeader, MessageType.Information);

                Data.Update_IsUpdateAvailable = false;

                return;
            }

            // Indicate that a new update is available
            Data.Update_IsUpdateAvailable = true;

            // Run as new task to mark this operation as finished
            Task.Run(async () =>
            {
                try
                {
                    bool isBeta = result.IsBetaUpdate;

                    string message = String.Format(!isBeta 
                        ? Resources.Update_UpdateAvailable 
                        : Resources.Update_BetaUpdateAvailable, AppVersion, result.LatestVersion, result.DisplayNews);

                    if (await MessageUI.DisplayMessageAsync(message, Resources.Update_UpdateAvailableHeader, MessageType.Question, true))
                    {
                        // Launch the updater and run as admin if set to show under installed programs in under to update the Registry key
                        var succeeded = await Updater.UpdateAsync(result, false);

                        if (!succeeded)
                            Logger.Warn("The updater failed to update the program");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Updating RCP");
                    await MessageUI.DisplayMessageAsync(Resources.Update_Error, Resources.Update_ErrorHeader, MessageType.Error);
                }
            }).WithoutAwait("Updating");
        }
        finally
        {
            CheckingForUpdates = false;
        }
    }

    #endregion
}
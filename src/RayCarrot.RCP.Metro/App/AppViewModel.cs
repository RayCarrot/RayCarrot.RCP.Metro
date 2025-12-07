using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;

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
        AppUserData data)
    {
        // Set properties
        Updater = updater ?? throw new ArgumentNullException(nameof(updater));
        MessageUI = message ?? throw new ArgumentNullException(nameof(message));
        File = file ?? throw new ArgumentNullException(nameof(file));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
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

        // Create commands
        RestartAsAdminCommand = new AsyncRelayCommand(async () => await RestartAsync(asAdmin: true));
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
    private AppUserData Data { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The current app version
    /// </summary>
    public Version CurrentAppVersion => AppVersion;

    /// <summary>
    /// The previously recorded app version
    /// </summary>
    public Version? PrevAppVersion { get; set; }

    /// <summary>
    /// Indicates if this is the first time the app is launched
    /// </summary>
    public bool IsFirstLaunch => PrevAppVersion == null || PrevAppVersion == new Version(0, 0, 0, 0);

    /// <summary>
    /// Indicates if the current version is a beta version
    /// </summary>
    public bool IsBeta => true;

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

    public static Version AppVersion => new(14, 3, 0, 3);

    #endregion

    #region Public Methods

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
                    long size = 0;
                    foreach (Uri item in inputSources)
                    {
                        WebRequest webRequest = WebRequest.Create(item);
                        webRequest.Method = "HEAD";

                        using WebResponse webResponse = webRequest.GetResponse();
                        size += Convert.ToInt64(webResponse.Headers.Get("Content-Length"));
                    }

                    string sizeString = BinaryHelpers.BytesToString(size);

                    Logger.Debug("The size of the download has been retrieved as {0}", sizeString);

                    string msg = isGame ? Resources.DownloadGame_ConfirmSize : Resources.Download_ConfirmSize;

                    if (!await MessageUI.DisplayMessageAsync(String.Format(msg, sizeString), Resources.Download_ConfirmHeader, MessageType.Question, true))
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
    /// Restarts the Rayman Control Panel with the specified arguments and optionally as admin
    /// </summary>
    /// <param name="asAdmin">Indicates if the app should run as admin</param>
    /// <param name="args">The launch arguments to use</param>
    public async Task RestartAsync(bool asAdmin, params string[] args)
    {
        await File.LaunchFileAsync(Data.App_ApplicationPath, asAdmin, $"{args.Prepend("-restart").Select(x => $"\"{x}\"").JoinItems(" ")}");
        await App.Current.ShutdownAppAsync(true);
    }

    public async Task RunAdminWorkerAsync(string mainArgument, params string[] args)
    {
        string argsString = $"{AdminWorker.MainArg} {args.Prepend(mainArgument).Select(x => $"\"{x}\"").JoinItems(" ")}";
        Process p = await File.LaunchFileAsync(Data.App_ApplicationPath, true, argsString);

        await p.WaitForExitAsync();
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
            await RestartAsync(asAdmin: true);
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
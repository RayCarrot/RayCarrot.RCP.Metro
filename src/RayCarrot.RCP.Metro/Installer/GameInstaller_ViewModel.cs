#nullable disable
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the game installer
/// </summary>
public class GameInstaller_ViewModel : UserInputViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="game"></param>
    public GameInstaller_ViewModel(Games game)
    {
        Logger.Info("An installation has been requested for the game {0}", game);

        // Create the commands
        InstallCommand = new AsyncRelayCommand(InstallAsync);
        CancelCommand = new AsyncRelayCommand(AttemptCancelAsync);

        // Set game
        Game = game;

        // Set title
        Title = String.Format(Resources.Installer_Title, game.GetGameInfo().DisplayName);

        // Create cancellation token source
        CancellationTokenSource = new CancellationTokenSource();

        // Get images
        GameLogoSource = $"{AppViewModel.WPFApplicationBasePath}Img/GameLogos/{game}_Logo.png";
        Gifs = game.GetGameInfo().InstallerGifs;

        // Default the install directory
        InstallDir = Environment.SpecialFolder.ProgramFiles.GetFolderPath();

        // Default image source to an empty string
        CurrentGifImageSource = String.Empty;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _createShortcutsForAllUsers;

    #endregion

    #region Public Properties

    /// <summary>
    /// The game to install
    /// </summary>
    public Games Game { get; }

    /// <summary>
    /// The cancellation token source for the installation
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; }

    /// <summary>
    /// The gif images to show
    /// </summary>
    public string[] Gifs { get; }

    /// <summary>
    /// The current gif image source to show
    /// </summary>
    public string CurrentGifImageSource { get; set; }

    /// <summary>
    /// The game logo image source
    /// </summary>
    public string GameLogoSource { get; }

    /// <summary>
    /// Indicates if the current gif image should be shown
    /// </summary>
    public bool ShowGifImage { get; set; }

    /// <summary>
    /// A flag indicating if the installer is running
    /// </summary>
    public bool InstallerRunning { get; set; }

    /// <summary>
    /// The selected directory to install the game to
    /// </summary>
    public FileSystemPath InstallDir { get; set; }

    /// <summary>
    /// Indicates if a desktop shortcut should be created
    /// </summary>
    public bool CreateDesktopShortcut { get; set; } = true;

    /// <summary>
    /// Indicates if a start menu shortcut should be created
    /// </summary>
    public bool CreateStartMenuShortcut { get; set; } = true;

    /// <summary>
    /// Indicates if the shortcuts should be created for all users
    /// </summary>
    public bool CreateShortcutsForAllUsers
    {
        get => _createShortcutsForAllUsers;
        set
        {
            if (value && !Services.App.IsRunningAsAdmin)
            {
                Task.Run(async () => await Services.MessageUI.DisplayMessageAsync(Resources.Installer_InstallAllUsersError, Resources.Installer_InstallAllUsersErrorHeader, MessageType.Warning));

                return;
            }

            _createShortcutsForAllUsers = value;
        }
    }

    /// <summary>
    /// The current total progress
    /// </summary>
    public double TotalCurrentProgress { get; set; }

    /// <summary>
    /// The max total progress
    /// </summary>
    public double TotalMaxProgress { get; set; } = 100;

    /// <summary>
    /// The current item progress
    /// </summary>
    public double ItemCurrentProgress { get; set; }

    /// <summary>
    /// The max item progress
    /// </summary>
    public double ItemMaxProgress { get; set; } = 100;

    /// <summary>
    /// The current item info
    /// </summary>
    public string CurrentItemInfo { get; set; }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the installation status is updated
    /// </summary>
    public event StatusUpdateEventHandler StatusUpdated;

    /// <summary>
    /// Occurs when the installation is complete or canceled
    /// </summary>
    public event EventHandler InstallationComplete;

    #endregion

    #region Commands

    public ICommand InstallCommand { get; }

    public ICommand CancelCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Attempts to cancel the installation
    /// </summary>
    public async Task AttemptCancelAsync()
    {
        if (CancellationTokenSource.IsCancellationRequested)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.Download_OperationCanceling, Resources.Download_OperationCancelingHeader, MessageType.Information);
            return;
        }

        if (await Services.MessageUI.DisplayMessageAsync(Resources.Installer_CancelQuestion, Resources.Installer_CancelQuestionHeader, MessageType.Question, true))
        {
            Logger.Info("The installation has been requested to cancel");
            CancellationTokenSource.Cancel();
        }
    }

    /// <summary>
    /// Begins refreshing the gif images
    /// </summary>
    /// <returns>The task</returns>
    public async Task RefreshGifsAsync()
    {
        Logger.Info("The gif images are being refreshed for the installation");

        ShowGifImage = true;

        while (InstallerRunning)
        {
            foreach (var img in Gifs)
            {
                if (!InstallerRunning)
                    break;

                CurrentGifImageSource = img;

                await Task.Delay(TimeSpan.FromSeconds(7));
            }
        }

        ShowGifImage = false;
    }

    /// <summary>
    /// Installs the game
    /// </summary>
    /// <returns>The task</returns>
    public async Task InstallAsync()
    {
        // Make sure the installer is not already running
        if (InstallerRunning)
        {
            Logger.Warn("A requested installation was canceled due to already running");
            return;
        }

        // Make sure the selected directory exists
        if (!InstallDir.DirectoryExists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.Installer_InvalidDirectory, Resources.Installer_InvalidDirectoryHeader, MessageType.Error);
            return;
        }

        // Make sure write permission is granted to the selected directory
        if (!Services.File.CheckDirectoryWriteAccess(InstallDir))
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.Installer_DirMissingWritePermission, Resources.Installer_DirMissingWritePermissionHeader, MessageType.Error);
            return;
        }

        try
        {
            // Flag that the installer is running
            InstallerRunning = true;

            // Begin refreshing gifs
            Task.Run(async () => await RefreshGifsAsync()).WithoutAwait("Refreshed installed GIFs");

            // Get the game display name
            var displayName = Game.GetGameInfo().DisplayName;

            // Get the output path
            FileSystemPath output = InstallDir + displayName;

            // Create the installer
            using var installer = new GameInstaller(new GameInstaller_Data(Game.GetInstallerItems(output), output, CancellationTokenSource.Token));

            // Subscribe to when the status is updated
            installer.StatusUpdated += Installer_StatusUpdated;

            // Run the installer
            var result = await Task.Run(async () => await installer.InstallAsync());

            Logger.Info("The installation finished with the result of {0}", result);

            // Make sure the result was successful
            if (result == GameInstaller_Result.Successful)
            {
                // Add the game
                await Services.App.AddNewGameAsync(Game, GameType.Win32, output);

                // Add game to installed games
                Services.Data.Game_InstalledGames.Add(Game);

                // Refresh
                await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, RefreshFlags.GameCollection));

                if (CreateDesktopShortcut)
                    await AddShortcutAsync((CreateShortcutsForAllUsers ? Environment.SpecialFolder.CommonDesktopDirectory : Environment.SpecialFolder.Desktop).GetFolderPath(), String.Format(Resources.Installer_ShortcutName, displayName));

                if (CreateStartMenuShortcut)
                    await AddShortcutAsync((CreateShortcutsForAllUsers ? Environment.SpecialFolder.CommonStartMenu : Environment.SpecialFolder.StartMenu).GetFolderPath() + "Programs", displayName);

                // Helper method for creating a shortcut
                async Task AddShortcutAsync(FileSystemPath dir, string shortcutName)
                {
                    try
                    {
                        Game.GetManager().CreateGameShortcut(shortcutName, dir);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Creating game shortcut for {0} from installer", Game);
                        await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameShortcut_Error, Resources.GameShortcut_ErrorHeader);
                    }
                }
            }

            switch (result)
            {
                case GameInstaller_Result.Successful:
                    await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_Success, displayName), Resources.Installer_SuccessHeader, MessageType.Success);
                    break;

                default:
                case GameInstaller_Result.Failed:
                    await Services.MessageUI.DisplayMessageAsync(Resources.Installer_Failed, Resources.Installer_FailedHeader, MessageType.Error);
                    break;

                case GameInstaller_Result.Canceled:
                    await Services.MessageUI.DisplayMessageAsync(Resources.Installer_Canceled, Resources.Installer_FailedHeader, MessageType.Information);
                    break;
            }
        }
        finally
        {
            InstallerRunning = false;
            InstallationComplete?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Event Handlers

    private void Installer_StatusUpdated(object sender, OperationProgressEventArgs e)
    {
        StatusUpdated?.Invoke(sender, e);

        // Update the progress
        TotalCurrentProgress = e.Progress.TotalProgress.Current;
        TotalMaxProgress = e.Progress.TotalProgress.Max;

        ItemCurrentProgress = e.Progress.ItemProgress.Current;
        ItemMaxProgress = e.Progress.ItemProgress.Max;

        // Set current item info
        if (e.Progress.CurrentItem is GameInstaller_Item item)
            CurrentItemInfo= item.OutputPath;
    }

    #endregion
}
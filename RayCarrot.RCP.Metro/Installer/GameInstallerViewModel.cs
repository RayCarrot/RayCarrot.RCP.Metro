using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the game installer
    /// </summary>
    public class GameInstallerViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game"></param>
        public GameInstallerViewModel(Games game)
        {
            RCFCore.Logger?.LogInformationSource($"An installation has been requested for the game {game}");

            // Create the commands
            InstallCommand = new AsyncRelayCommand(InstallAsync);
            CancelCommand = new AsyncRelayCommand(AttemptCancelAsync);

            // Set game
            Game = game;

            // Create cancellation token source
            CancellationTokenSource = new CancellationTokenSource();

            // Get images
            GameLogoSource = $"{AppViewModel.ApplicationBasePath}Img/GameLogos/{game}_Logo.png";
            Gifs = game.GetInstallerGifs();

            // Default the install directory
            InstallDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            // Default gif source to an empty string
            CurrentGifImageSource = String.Empty;
        }

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
                if (!value || App.IsRunningAsAdmin)
                {
                    _createShortcutsForAllUsers = value;
                    return;
                }

                Task.Run(async () => await RCFUI.MessageUI.DisplayMessageAsync("You need to run the program as administrator in order to install for all users", "Missing permissions", MessageType.Warning));
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
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.Installer_CancelAlreadyRequested, Resources.Installer_CancelAlreadyRequestedHeader, MessageType.Information);
                return;
            }

            if (await RCFUI.MessageUI.DisplayMessageAsync(Resources.Installer_CancelQuestion, Resources.Installer_CancelQuestionHeader, MessageType.Question, true))
            {
                RCFCore.Logger?.LogInformationSource($"The installation has been requested to cancel");
                CancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Begins refreshing the gif images
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshGifsAsync()
        {
            RCFCore.Logger?.LogInformationSource($"The gif images are being refreshed for the installation");

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
                RCFCore.Logger?.LogWarningSource($"A requested installation was canceled due to already running");
                return;
            }

            // Make sure the selected directory exists
            if (!InstallDir.DirectoryExists)
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.Installer_InvalidDirectory, Resources.Installer_InvalidDirectoryHeader, MessageType.Error);
                return;
            }

            // Make sure write permission is granted to the selected directory
            if (!RCFRCP.File.CheckDirectoryWriteAccess(InstallDir))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.Installer_DirMissingWritePermission, Resources.Installer_DirMissingWritePermissionHeader, MessageType.Error);
                return;
            }

            try
            {
                // Flag that the installer is running
                InstallerRunning = true;

                // Begin refreshing gifs
                _ = Task.Run(async () => await RefreshGifsAsync());

                // Get the output path
                FileSystemPath output = InstallDir + Game.GetDisplayName();

                // Create the installer
                var installer = new RayGameInstaller(new RayGameInstallerData(Game.GetInstallerItems(output), output, CancellationTokenSource.Token));

                // Subscribe to when the status is updated
                installer.StatusUpdated += Installer_StatusUpdated;

                // Run the installer
                var result = await Task.Run(async () => await installer.InstallAsync());

                RCFCore.Logger?.LogInformationSource($"The installation finished with the result of {result}");

                // Check if the game is Rayman 2
                if (result == RayGameInstallerResult.Successful && Game == Games.Rayman2)
                {
                    try
                    {
                        // Write the GOG exe file
                        File.WriteAllBytes(output + "Rayman2.exe", Files.Rayman2_GOG);

                        // Write the GOG config file
                        File.WriteAllBytes(output + "GXSetup.exe", Files.GXSetup_GOG);

                        // Write default ubi.ini file
                        File.WriteAllText(output + "ubi.ini", Files.ubi);

                        // Delete unnecessary files
                        RCFRCP.File.DeleteFile(output + "RAYMAN2.ICD");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\ANIMS0.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\ANIMS1.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\ANIMS2.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\GRAPHICS0.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\GRAPHICS1.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\GRAPHICS2.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\LEVELS1.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\LEVELS2.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MAP0.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MAP1.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MAP2.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MENU0.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MENU1.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MENU2.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MUSIC0.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MUSIC1.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\MUSIC2.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\SOUNDS0.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\SOUNDS1.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\SOUNDS2.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\TEXTURES0.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\TEXTURES1.DAT");
                        RCFRCP.File.DeleteFile(output + @"Data\World\Levels\TEXTURES2.DAT");
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Applying R2 installer extras");
                    }
                }

                // Make sure the result was successful
                if (result == RayGameInstallerResult.Successful)
                {
                    // Remove the game if it has been added
                    if (Game.IsAdded())
                        await App.RemoveGameAsync(Game, true);

                    // Add the game
                    await App.AddNewGameAsync(Game, GameType.Win32, output);

                    // Get the launch info
                    var launchInfo = Game.GetGameManager().GetLaunchInfo();

                    if (CreateDesktopShortcut)
                        await AddShortcutAsync(Environment.GetFolderPath(CreateShortcutsForAllUsers ? Environment.SpecialFolder.CommonDesktopDirectory : Environment.SpecialFolder.Desktop), String.Format(Resources.Installer_ShortcutName, Game.GetDisplayName()));

                    if (CreateStartMenuShortcut)
                        await AddShortcutAsync(Path.Combine(Environment.GetFolderPath(CreateShortcutsForAllUsers ? Environment.SpecialFolder.CommonStartMenu : Environment.SpecialFolder.StartMenu), "Programs"), Game.GetDisplayName());

                    async Task AddShortcutAsync(FileSystemPath dir, string shortcutName) => await RCFRCP.File.CreateFileShortcutAsync(shortcutName, dir, launchInfo.Path, launchInfo.Args);
                }

                switch (result)
                {
                    case RayGameInstallerResult.Successful:
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Installer_Success, Game.GetDisplayName()), Resources.Installer_SuccessHeader, MessageType.Success);
                        break;

                    default:
                    case RayGameInstallerResult.Failed:
                        await RCFUI.MessageUI.DisplayMessageAsync(Resources.Installer_Failed, Resources.Installer_FailedHeader, MessageType.Error);
                        break;

                    case RayGameInstallerResult.Canceled:
                        await RCFUI.MessageUI.DisplayMessageAsync(Resources.Installer_Canceled, Resources.Installer_FailedHeader, MessageType.Information);
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
            if (e.Progress.CurrentItem is RayGameInstallItem item)
                CurrentItemInfo= item.OutputPath;
        }

        #endregion
    }
}
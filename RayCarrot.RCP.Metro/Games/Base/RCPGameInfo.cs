using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.RCP.UI;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base for Rayman Control Panel game data
    /// </summary>
    public abstract class RCPGameInfo : RCPGame
    {
        #region Protected Constants

        /// <summary>
        /// The group name to use for a dialog which requires reading/writing to a ubi.ini file
        /// </summary>
        protected const string UbiIniFileGroupName = "ubini-config";

        #endregion

        #region Public Properties

        /// <summary>
        /// The icon source for the game
        /// </summary>
        public string IconSource => $"{APIControllerUISettings.GetSettings().ApplicationBasePath}Img/GameIcons/{Game}.png";

        /// <summary>
        /// Indicates if the game can be uninstalled
        /// </summary>
        public bool CanBeUninstalled => (CanBeDownloaded || CanBeInstalledFromDisc) && RCFRCP.Data.InstalledGames.Contains(Game);

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The game display name
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// The game backup name
        /// </summary>
        public abstract string BackupName { get; }

        /// <summary>
        /// Gets the default file name for launching the game, if available
        /// </summary>
        public abstract string DefaultFileName { get; }

        /// <summary>
        /// The category for the game
        /// </summary>
        public abstract GameCategory Category { get; }

        #endregion

        #region Protected Virtual Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected virtual IList<BackupDir> GetBackupDirectories => null;

        #endregion

        #region Public Virtual Properties

        /// <summary>
        /// The options UI, if any is available
        /// </summary>
        public virtual FrameworkElement OptionsUI => null;

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public virtual FrameworkElement ConfigUI => null;

        /// <summary>
        /// The progression view model, if any is available
        /// </summary>
        public virtual BaseProgressionViewModel ProgressionViewModel => null;

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public virtual IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        /// <summary>
        /// Gets the backup infos from the directories specified from <see cref="GetBackupDirectories"/>
        /// </summary>
        public virtual IList<IBackupInfo> GetBackupInfos
        {
            get
            {
                var backupDirectories = GetBackupDirectories;

                return backupDirectories == null
                    ? new List<IBackupInfo>()
                    : new List<IBackupInfo>()
                    {
                        new BaseBackupInfo(BackupName, backupDirectories, DisplayName)
                    };
            }
        }

        /// <summary>
        /// The group names to use for the options, config and utility dialog
        /// </summary>
        public virtual IEnumerable<string> DialogGroupNames => new string[0];

        /// <summary>
        /// Indicates if the game can be located. If set to false the game is required to be downloadable.
        /// </summary>
        public virtual bool CanBeLocated => true;

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public virtual bool CanBeDownloaded => false;

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public virtual IList<Uri> DownloadURLs => null;

        /// <summary>
        /// Indicates if the game can be installed from a disc in this program
        /// </summary>
        public virtual bool CanBeInstalledFromDisc => false;

        /// <summary>
        /// The .gif files to use during the game installation if installing from a disc
        /// </summary>
        public virtual string[] InstallerGifs => null;

        /// <summary>
        /// The directories to remove when uninstalling. This should not include the game install directory as that is included by default.
        /// </summary>
        public virtual IEnumerable<FileSystemPath> UninstallDirectories => null;

        /// <summary>
        /// The files to remove when uninstalling
        /// </summary>
        public virtual IEnumerable<FileSystemPath> UninstallFiles => null;

        #endregion

        #region Public Virtual Methods

        /// <summary>
        /// Gets the applied utilities for the specified game
        /// </summary>
        /// <returns>The applied utilities</returns>
        public virtual Task<IList<string>> GetAppliedUtilitiesAsync()
        {
            return Task.FromResult<IList<string>>(RCFRCP.App.GetUtilities(Game).SelectMany(x => x.GetAppliedUtilities()).ToArray());
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets a display view model for the game
        /// </summary>
        /// <returns>A new display view model</returns>
        public GameDisplayViewModel GetDisplayViewModel()
        {
            try
            {
                if (IsAdded)
                {
                    var actions = new List<OverflowButtonItemViewModel>();

                    // Get the manager
                    var manager = Game.GetManager(Game.GetGameType());

                    // Add launch options if set to do so
                    if (Game.GetLaunchMode() == GameLaunchMode.AsAdminOption)
                    {
                        actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_RunAsAdmin, PackIconMaterialKind.Security, new AsyncRelayCommand(async () => await Game.GetManager().LaunchGameAsync(true))));

                        actions.Add(new OverflowButtonItemViewModel());
                    }

                    // Get the Game links
                    var links = GetGameFileLinks?.Where(x => x.Path.FileExists).ToArray();

                    // Add links if there are any
                    if (links?.Any() ?? false)
                    {
                        actions.AddRange(links.
                            Select(x =>
                            {
                            // Get the path
                            string path = x.Path;

                            // Create the command
                            var command = new AsyncRelayCommand(async () => (await RCFRCPA.File.LaunchFileAsync(path))?.Dispose());

                                if (x.Icon != PackIconMaterialKind.None)
                                    return new OverflowButtonItemViewModel(x.Header, x.Icon, command);

                                try
                                {
                                    return new OverflowButtonItemViewModel(x.Header, new FileSystemPath(x.Path).GetIconOrThumbnail(ShellThumbnailSize.Small).ToImageSource(), command);
                                }
                                catch (Exception ex)
                                {
                                    ex.HandleError("Getting file icon for overflow button item");
                                    return new OverflowButtonItemViewModel(x.Header, x.Icon, command);
                                }
                            }));

                        actions.Add(new OverflowButtonItemViewModel());
                    }

                    // Get additional items
                    var additionalItems = manager.GetAdditionalOverflowButtonItems;

                    // Add the items if there are any
                    if (additionalItems.Any())
                    {
                        actions.AddRange(additionalItems);

                        actions.Add(new OverflowButtonItemViewModel());
                    }

                    // Add open location
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_OpenLocation, PackIconMaterialKind.FolderOutline, new AsyncRelayCommand(async () =>
                    {
                        // Get the install directory
                        var instDir = Game.GetInstallDir();

                        // Select the file in Explorer if it exists
                        if ((instDir + DefaultFileName).FileExists)
                            instDir += DefaultFileName;

                        // Open the location
                        await RCFRCPA.File.OpenExplorerLocationAsync(instDir);

                        RCFCore.Logger?.LogTraceSource($"The Game {Game} install location was opened");
                    }), UserLevel.Advanced));

                    actions.Add(new OverflowButtonItemViewModel(UserLevel.Advanced));

                    // Add Game options
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Options, PackIconMaterialKind.SettingsOutline, new RelayCommand(() =>
                    {
                        RCFCore.Logger?.LogTraceSource($"The Game {Game} options dialog is opening...");
                        GameOptions.Show(Game);
                    })));

                    return new GameDisplayViewModel(Game, DisplayName, IconSource, new ActionItemViewModel(Resources.GameDisplay_Launch, PackIconMaterialKind.PlayOutline, new AsyncRelayCommand(async () => await Game.GetManager().LaunchGameAsync(false))), actions);
                }
                else
                {
                    var actions = new List<OverflowButtonItemViewModel>();

                    OverflowButtonItemViewModel downloadItem = null;

                    if (CanBeDownloaded)
                    {
                        downloadItem = new OverflowButtonItemViewModel(Resources.GameDisplay_CloudInstall, PackIconMaterialKind.CloudDownloadOutline, new AsyncRelayCommand(async () => await DownloadGameAsync()));

                        if (CanBeLocated)
                        {
                            actions.Add(downloadItem);
                            actions.Add(new OverflowButtonItemViewModel());
                        }
                    }

                    // Get the purchase links
                    var links = Game.
                        // Get all available managers
                        GetManagers().
                        // Get the purchase links
                        SelectMany(x => x.GetGamePurchaseLinks);

                    // Add links
                    actions.AddRange(links.
                        Select(x =>
                        {
                            // Get the path
                            string path = x.Path;

                            // Create the command
                            var command = new AsyncRelayCommand(async () => (await RCFRCPA.File.LaunchFileAsync(path))?.Dispose());

                            // Return the item
                            return new OverflowButtonItemViewModel(x.Header, x.Icon, command);
                        }));

                    // Add disc installer options for specific Games
                    if (CanBeInstalledFromDisc)
                    {
                        // Add separator if there are previous actions
                        if (actions.Any())
                            actions.Add(new OverflowButtonItemViewModel());

                        // Add disc installer action
                        actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_DiscInstall, PackIconMaterialKind.Disc, new RelayCommand(() =>
                            // NOTE: This is a blocking dialog
                            new GameInstaller(Game).ShowDialog())));
                    }

                    // Return the view model
                    return new GameDisplayViewModel(Game, DisplayName, IconSource, 
                        CanBeLocated 
                            ? new ActionItemViewModel(Resources.GameDisplay_Locate, PackIconMaterialKind.FolderOutline, new AsyncRelayCommand(async () => await LocateGameAsync()))
                            : downloadItem, actions);
                }
            }
            catch (Exception ex)
            {
                ex.HandleCritical("Getting game display view model");
                throw;
            }
        }

        /// <summary>
        /// Gets a type for the game, or null if the operation was canceled
        /// </summary>
        /// <returns>The type or null if the operation was canceled</returns>
        public async Task<GameTypeSelectionResult> GetGameTypeAsync()
        {
            // Get the available types
            var types = RCFRCP.App.GameManagers[Game].Keys.ToArray();

            // If only one type, return that
            if (types.Length == 1)
                return new GameTypeSelectionResult()
                {
                    CanceledByUser = false,
                    SelectedType = types.First()
                };

            // Create the view model
            var vm = new GameTypeSelectionViewModel()
            {
                Title = Resources.App_SelectGameTypeHeader
            };

            // Enumerate the available types
            foreach (var type in types)
            {
                switch (type)
                {
                    case GameType.Win32:
                        vm.AllowWin32 = true;
                        break;

                    case GameType.Steam:
                        vm.AllowSteam = true;
                        break;

                    case GameType.WinStore:
                        vm.AllowWinStore = true;
                        break;

                    case GameType.DosBox:
                        vm.AllowDosBox = true;
                        break;

                    case GameType.EducationalDosBox:
                        vm.AllowEducationalDosBox = true;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            // Create and show the dialog and return the result
            return await RCFRCP.UI.SelectGameTypeAsync(vm);
        }

        /// <summary>
        /// Allows the user to locate the game and add it
        /// </summary>
        /// <returns>The task</returns>
        public async Task LocateGameAsync()
        {
            try
            {
                RCFCore.Logger?.LogTraceSource($"The game {Game} is being located...");

                var typeResult = await GetGameTypeAsync();

                if (typeResult.CanceledByUser)
                    return;

                RCFCore.Logger?.LogInformationSource($"The game {Game} type has been detected as {typeResult.SelectedType}");

                await Game.GetManager(typeResult.SelectedType).LocateAddGameAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Locating game");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.LocateGame_Error, Resources.LocateGame_ErrorHeader);
            }
        }

        /// <summary>
        /// Allows the user to download the game and add it
        /// </summary>
        /// <returns>The task</returns>
        public async Task DownloadGameAsync()
        {
            try
            {
                RCFCore.Logger?.LogTraceSource($"The game {Game} is being downloaded...");

                // Get the game directory
                var gameDir = CommonPaths.GamesBaseDir + Game.ToString();

                // Download the game
                var downloaded = await RCFRCP.App.DownloadAsync(DownloadURLs, true, gameDir, true);

                if (!downloaded)
                    return;

                // NOTE: Downloaded games can currently only be of type Win32
                // Add the game
                await RCFRCP.App.AddNewGameAsync(Game, GameType.Win32, gameDir);

                // Add game to installed games
                RCFRCP.Data.InstalledGames.Add(Game);

                // Refresh
                await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, true, false, false, false));

                RCFCore.Logger?.LogTraceSource($"The game {Game} has been downloaded");

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.GameInstall_Success, DisplayName), Resources.GameInstall_SuccessHeader);
            }
            catch (Exception ex)
            {
                ex.HandleError("Downloading game");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.GameInstall_Error, DisplayName), Resources.GameInstall_ErrorHeader);
            }
        }

        #endregion
    }
}
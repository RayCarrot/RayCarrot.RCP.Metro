#nullable disable
using NLog;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base for Rayman Control Panel game data
/// </summary>
public abstract class GameInfo : BaseGameData
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

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
    public string IconSource => $"{AppViewModel.WPFApplicationBasePath}Img/GameIcons/{Game}.png";

    /// <summary>
    /// Indicates if the game can be uninstalled
    /// </summary>
    public bool CanBeUninstalled => Services.Data.Game_InstalledGames.Contains(Game);

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
    protected virtual IList<GameBackups_Directory> GetBackupDirectories => null;

    #endregion

    #region Public Virtual Properties

    /// <summary>
    /// The options UI, if any is available
    /// </summary>
    public virtual FrameworkElement OptionsUI => null;

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public virtual GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => null;

    /// <summary>
    /// The progression view model, if any is available
    /// </summary>
    public virtual GameProgression_BaseViewModel ProgressionViewModel => null;

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public virtual IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public virtual string RayMapURL => null;

    /// <summary>
    /// Gets the backup infos from the directories specified from <see cref="GetBackupDirectories"/>
    /// </summary>
    public virtual IList<IGameBackups_BackupInfo> GetBackupInfos
    {
        get
        {
            var backupDirectories = GetBackupDirectories;

            return backupDirectories == null
                ? new List<IGameBackups_BackupInfo>()
                : new List<IGameBackups_BackupInfo>()
                {
                    new GameBackups_BackupInfo(BackupName, backupDirectories, DisplayName)
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
    /// The type of game if it can be downloaded
    /// </summary>
    public virtual GameType DownloadType => GameType.Win32;

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

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public virtual bool HasArchives => false;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public virtual IArchiveDataManager GetArchiveDataManager => null;

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public virtual FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => null;

    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public virtual Emulator Emulator => null;

    #endregion

    #region Public Virtual Methods

    /// <summary>
    /// Gets the applied utilities for the specified game
    /// </summary>
    /// <returns>The applied utilities</returns>
    public virtual Task<IList<string>> GetAppliedUtilitiesAsync()
    {
        return Task.FromResult<IList<string>>(Services.App.GetUtilities(Game).SelectMany(x => x.GetAppliedUtilities()).ToArray());
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Gets a display view model for the game
    /// </summary>
    /// <returns>A new display view model</returns>
    public Page_Games_GameViewModel GetDisplayViewModel()
    {
        try
        {
            if (IsAdded)
            {
                var actions = new List<OverflowButtonItemViewModel>();

                // Get the manager
                var manager = Game.GetManager(Game.GetGameType());

                // Add launch options if set to do so
                if (Game.GetLaunchMode() == UserData_GameLaunchMode.AsAdminOption)
                {
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_RunAsAdmin, GenericIconKind.GameDisplay_Admin, new AsyncRelayCommand(async () => await Game.GetManager().LaunchGameAsync(true))));

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
                            var command = new AsyncRelayCommand(async () => (await Services.File.LaunchFileAsync(path, arguments: x.Arguments))?.Dispose());

                            if (x.Icon != GenericIconKind.None)
                                return new OverflowButtonItemViewModel(x.Header, x.Icon, command);

                            try
                            {
                                return new OverflowButtonItemViewModel(x.Header, WindowsHelpers.GetIconOrThumbnail(x.Path, ShellThumbnailSize.Small).ToImageSource(), command);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "Getting file icon for overflow button item");
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

                // Add RayMap link
                if (RayMapURL != null)
                {
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Raymap, GenericIconKind.GameDisplay_Map, new AsyncRelayCommand(async () => (await Services.File.LaunchFileAsync(RayMapURL))?.Dispose())));
                    actions.Add(new OverflowButtonItemViewModel());
                }

                // Add open archive
                if (HasArchives)
                {
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_Archives, GenericIconKind.GameDisplay_Archive, new AsyncRelayCommand(async () =>
                    {
                        // Show the archive explorer
                        await Services.UI.ShowArchiveExplorerAsync(GetArchiveDataManager, GetArchiveFilePaths(Game.GetInstallDir()).Where(x => x.FileExists).ToArray());
                    }), UserLevel.Advanced));
                }

                // Add open location
                actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_OpenLocation, GenericIconKind.GameDisplay_Location, new AsyncRelayCommand(async () =>
                {
                    // Get the install directory
                    var instDir = Game.GetInstallDir();

                    // Select the file in Explorer if it exists
                    if ((instDir + DefaultFileName).FileExists)
                        instDir += DefaultFileName;

                    // Open the location
                    await Services.File.OpenExplorerLocationAsync(instDir);

                    Logger.Trace("The Game {0} install location was opened", Game);
                }), UserLevel.Advanced));

                actions.Add(new OverflowButtonItemViewModel(UserLevel.Advanced));

                // Add Game options
                var optionsAction = new OverflowButtonItemViewModel(Resources.GameDisplay_Options, GenericIconKind.GameDisplay_Config, new AsyncRelayCommand(async () =>
                {
                    Logger.Trace("The Game {0} options dialog is opening...", Game);
                    await GameOptionsDialog.ShowAsync(Game);
                }));

                actions.Add(optionsAction);

                return new Page_Games_GameViewModel(
                    game: Game, 
                    displayName: DisplayName, 
                    iconSource: IconSource, 
                    mainAction: new ActionItemViewModel(Resources.GameDisplay_Launch, GenericIconKind.GameDisplay_Play, new AsyncRelayCommand(async () => await Game.GetManager().LaunchGameAsync(false))), 
                    secondaryAction: optionsAction, 
                    launchActions: actions);
            }
            else
            {
                var actions = new List<OverflowButtonItemViewModel>();

                OverflowButtonItemViewModel downloadItem = null;

                if (CanBeDownloaded)
                {
                    downloadItem = new OverflowButtonItemViewModel(Resources.GameDisplay_CloudInstall, GenericIconKind.GameDisplay_Download, new AsyncRelayCommand(async () => await DownloadGameAsync()));

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
                        var command = new AsyncRelayCommand(async () => (await Services.File.LaunchFileAsync(path))?.Dispose());

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
                    actions.Add(new OverflowButtonItemViewModel(Resources.GameDisplay_DiscInstall, GenericIconKind.GameDisplay_DiscInstall, new AsyncRelayCommand(async () =>
                    {
                        // Show and run the installer
                        await Services.DialogBaseManager.ShowDialogWindowAsync(new GameInstaller_Window(Game));
                    })));
                }

                // If the last option is a separator, remove it
                if (actions.LastOrDefault()?.IsSeparator == true)
                    actions.RemoveAt(actions.Count - 1);

                // Create the main action
                var mainAction = CanBeLocated
                    ? new ActionItemViewModel(Resources.GameDisplay_Locate, GenericIconKind.GameDisplay_Location, new AsyncRelayCommand(async () => await LocateGameAsync()))
                    : downloadItem;

                // Return the view model
                return new Page_Games_GameViewModel(Game, DisplayName, IconSource, mainAction, null, actions);
            }
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Getting game display view model");
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
        var types = Services.App.GamesManager.GameManagers[Game].Keys.ToArray();

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
            if (type == GameType.Win32)
                vm.AllowWin32 = true;
            else if (type == GameType.Steam)
                vm.AllowSteam = true;
            else if (type == GameType.WinStore)
                vm.AllowWinStore = true;
            else if (type == GameType.DosBox)
                vm.AllowDosBox = true;
            else if (type == GameType.EducationalDosBox)
                vm.AllowEducationalDosBox = true;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        // Create and show the dialog and return the result
        return await Services.UI.SelectGameTypeAsync(vm);
    }

    /// <summary>
    /// Allows the user to locate the game and add it
    /// </summary>
    /// <returns>The task</returns>
    public async Task LocateGameAsync()
    {
        try
        {
            Logger.Trace("The game {0} is being located...", Game);

            var typeResult = await GetGameTypeAsync();

            if (typeResult.CanceledByUser)
                return;

            Logger.Info("The game {0} type has been detected as {1}", Game, typeResult.SelectedType);

            await Game.GetManager(typeResult.SelectedType).LocateAddGameAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Locating game");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.LocateGame_Error, Resources.LocateGame_ErrorHeader);
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
            Logger.Trace("The game {0} is being downloaded...", Game);

            // Get the game directory
            var gameDir = AppFilePaths.GamesBaseDir + Game.ToString();

            // Download the game
            var downloaded = await Services.App.DownloadAsync(DownloadURLs, true, gameDir, true);

            if (!downloaded)
                return;

            // Add the game
            await Services.App.AddNewGameAsync(Game, DownloadType, gameDir);

            // Add game to installed games
            Services.Data.Game_InstalledGames.Add(Game);

            // Refresh
            await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, RefreshFlags.GameCollection));

            Logger.Trace("The game {0} has been downloaded", Game);

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.GameInstall_Success, DisplayName), Resources.GameInstall_SuccessHeader);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Downloading game");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.GameInstall_Error, DisplayName), Resources.GameInstall_ErrorHeader);
        }
    }

    #endregion

    #region Data Types

    /// <summary>
    /// A game file link which can be accessed from the game
    /// </summary>
    public record GameFileLink(string Header, FileSystemPath Path, GenericIconKind Icon = GenericIconKind.None, string Arguments = null);

    #endregion
}
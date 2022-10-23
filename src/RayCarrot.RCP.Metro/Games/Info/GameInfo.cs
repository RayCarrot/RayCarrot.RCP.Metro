#nullable disable
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro;

// TODO-14: Clean up. Move code out of here. Rename to GameDescriptor? Move some things to modules/extensions.

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
    public string IconSource => $"{AppViewModel.WPFApplicationBasePath}Img/GameIcons/{IconName}.png";

    /// <summary>
    /// Indicates if the game can be uninstalled
    /// </summary>
    public bool CanBeUninstalled => Services.Data.Game_InstalledGames.Contains(Game);

    #endregion

    #region Public Abstract Properties

    /// <summary>
    /// The game display name
    /// </summary>
    public abstract string DisplayName { get; } // TODO-14: Localize & split up into short and long name

    /// <summary>
    /// The game backup name
    /// </summary>
    public virtual string BackupName => throw new InvalidOperationException($"The game {Game} has no backup name associated with it");

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
    /// The file name (without extensions) for the icon
    /// </summary>
    protected virtual string IconName => $"{Game}";

    #endregion

    #region Public Virtual Properties

    /// <summary>
    /// The options UI, if any is available
    /// </summary>
    public virtual FrameworkElement OptionsUI => null; // TODO-14: Don't use UI elements like this - use vm + template instead!

#nullable enable

    /// <summary>
    /// Gets the config page view model, if any is available
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the config page view model for</param>
    /// <returns>The config page view model of null if none is available</returns>
    public virtual GameOptionsDialog_ConfigPageViewModel? GetConfigPageViewModel(GameInstallation gameInstallation) => null;

#nullable disable

    /// <summary>
    /// The progression game view models
    /// </summary>
    public virtual IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels => Enumerable.Empty<ProgressionGameViewModel>();

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public virtual IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

    /// <summary>
    /// Optional RayMap URL
    /// </summary>
    public virtual string RayMapURL => null;

    /// <summary>
    /// The group names to use for the options, config and utility dialog
    /// </summary>
    public virtual IEnumerable<string> DialogGroupNames => new string[0];

    /// <summary>
    /// Indicates if the game is a demo
    /// </summary>
    public virtual bool IsDemo => false;

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

    public virtual bool AutoAddToJumpList => Category != GameCategory.Demo;

    public virtual bool AllowPatching => true;

    #endregion

    #region Public Virtual Methods

    /// <summary>
    /// Gets the applied utilities for the specified game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the utilities for</param>
    /// <returns>The applied utilities</returns>
    public virtual Task<IList<string>> GetAppliedUtilitiesAsync(GameInstallation gameInstallation)
    {
        return Task.FromResult<IList<string>>(Services.App.GetUtilities(gameInstallation).SelectMany(x => x.GetAppliedUtilities()).ToArray());
    }

    #endregion

    #region Public methods

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
            await Services.Games.AddGameAsync(Game, DownloadType, gameDir);

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
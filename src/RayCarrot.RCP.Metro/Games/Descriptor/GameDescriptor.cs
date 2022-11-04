using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NLog;
using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro;

// TODO-14: Move some things to optional modules/extensions/components, such as installer, archive, progression etc.
// TODO-14: Move descriptors to folders based on platform?
// TODO-14: Minimize the amount of methods here which do things by moving to manager classes. The descriptor should really only
//          be for providing data about the game.

/// <summary>
/// A game descriptor, providing data for a game
/// </summary>
public abstract class GameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Protected Constant Fields

    /// <summary>
    /// The group name to use for a dialog which requires reading/writing to a ubi.ini file
    /// </summary>
    protected const string UbiIniFileGroupName = "ubini-config"; // TODO-14: Remove from here

    #endregion

    #region Protected Properties

    /// <summary>
    /// The file name (without extensions) for the icon
    /// </summary>
    protected virtual string IconName => $"{LegacyGame}"; // TODO-14: Use enum for this or define file name string for each game

    #endregion

    #region Public Properties

    /// <summary>
    /// The game descriptor id. This should be unique and is used to identify this descriptor. Use this for logging as well.
    /// Note: There should never be hard-coded checks on this, which is why this is a string rather than an enum.
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// The game this descriptor is for. This is mainly for categorization and filter purposes.
    /// </summary>
    public abstract Game Game { get; }

    /// <summary>
    /// The platform the descriptor is for. This is mainly for categorization and filter purposes. The platform mainly defines
    /// how the game is handled in RCP and thus there is a separation between a Steam and standard Win32 release.
    /// </summary>
    public abstract GamePlatform Platform { get; }

    /// <summary>
    /// The category for the descriptor. This is only used for categorization purposes.
    /// </summary>
    public abstract GameCategory Category { get; }

    /// <summary>
    /// Indicates if the game is a demo
    /// </summary>
    public virtual bool IsDemo => false;

    /// <summary>
    /// The legacy games enum value
    /// </summary>
    public virtual Games? LegacyGame => null; // TODO-14: Minimize references to this

    /// <summary>
    /// The game display name
    /// </summary>
    public abstract string DisplayName { get; } // TODO-14: Localize & split up into short and long name
    //public abstract LocalizedString ShortDisplayName { get; }
    //public abstract LocalizedString LongDisplayName { get; }

    // TODO-14: Make this nullable instead?
    /// <summary>
    /// The game backup name
    /// </summary>
    public virtual string BackupName => throw new InvalidOperationException($"The game {Id} has no backup name associated with it");

    /// <summary>
    /// Gets the default file name for launching the game, if available
    /// </summary>
    public abstract string DefaultFileName { get; } // TODO-14: Remove from here - not all games have an exe!

    /// <summary>
    /// The icon source for the game
    /// </summary>
    public string IconSource => $"{AppViewModel.WPFApplicationBasePath}Img/GameIcons/{IconName}.png";

    /// <summary>
    /// An optional RayMap URL
    /// </summary>
    public virtual string? RayMapURL => null;

    /// <summary>
    /// The group names to use for the options, config and utility dialog
    /// </summary>
    public virtual IEnumerable<string> DialogGroupNames => Enumerable.Empty<string>(); // TODO-14: Change this

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
    public virtual IList<Uri>? DownloadURLs => null;

    /// <summary>
    /// Indicates if the game can be installed from a disc in this program
    /// </summary>
    public virtual bool HasGameInstaller => false;

    /// <summary>
    /// Indicates if the game should automatically be added to the jump list once added
    /// </summary>
    public virtual bool AutoAddToJumpList => !IsDemo;

    /// <summary>
    /// Indicates if the game supports the game patcher
    /// </summary>
    public virtual bool AllowPatching => true;

    /// <summary>
    /// Indicates if using <see cref="UserData_GameLaunchMode"/> is supported
    /// </summary>
    public abstract bool SupportsGameLaunchMode { get; } // TODO-14: This should not be here

    /// <summary>
    /// The directories to remove when uninstalling. This should not include the game install directory as that is included by default.
    /// </summary>
    public virtual IEnumerable<FileSystemPath> UninstallDirectories => Enumerable.Empty<FileSystemPath>();

    /// <summary>
    /// The files to remove when uninstalling
    /// </summary>
    public virtual IEnumerable<FileSystemPath> UninstallFiles => Enumerable.Empty<FileSystemPath>();

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public virtual bool HasArchives => false;

    // TODO-14: Rework this system. Each game descriptor should have a collection of possible emulators. These are then
    //          responsible for launching the game etc.
    /// <summary>
    /// An optional emulator to use for the game
    /// </summary>
    public virtual Emulator? Emulator => null;

    #endregion

    #region Protected Methods

    // TODO-14: Use this for all public APIs
    protected void VerifyGameInstallation(GameInstallation gameInstallation)
    {
        if (gameInstallation == null)
            throw new ArgumentNullException(nameof(gameInstallation));

        if (gameInstallation.Id != Id)
            throw new Exception($"The provided game installation ID {gameInstallation.Id} does not match {Id}");
    }

    /// <summary>
    /// Verifies if the game can launch
    /// </summary>
    /// <param name="gameInstallation">The game installation to check</param>
    /// <returns>True if the game can launch, otherwise false</returns>
    protected virtual Task<bool> VerifyCanLaunchAsync(GameInstallation gameInstallation) => Task.FromResult(true);

    /// <summary>
    /// The implementation for launching the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to launch</param>
    /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
    /// <returns>The launch result</returns>
    protected abstract Task<GameLaunchResult> LaunchAsync(GameInstallation gameInstallation, bool forceRunAsAdmin);

    /// <summary>
    /// Post launch operations for the game which launched
    /// </summary>
    /// <param name="process">The game process</param>
    /// <returns>The task</returns>
    protected virtual async Task PostLaunchAsync(Process? process)
    {
        // Dispose the process
        process?.Dispose();

        // Check if the application should close
        if (Services.Data.App_CloseAppOnGameLaunch)
            await App.Current.ShutdownAppAsync(false);
    }

    // TODO-14: Don't do this - an install location might be a file in case of roms
    /// <summary>
    /// Indicates if the game location is valid
    /// </summary>
    /// <param name="installLocation">The game install location</param>
    /// <returns>True if the game directory is valid, otherwise false</returns>
    protected virtual Task<bool> IsGameLocationValidAsync(FileSystemPath installLocation)
    {
        // Make sure the default file exists in the install directory
        return Task.FromResult((installLocation + DefaultFileName).FileExists);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// The options UI, if any is available
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the options for</param>
    /// <returns>The options UI or null if not available</returns>
    public virtual FrameworkElement? GetOptionsUI(GameInstallation gameInstallation) => null; // TODO-14: Don't use UI elements like this - use vm + template instead!

    /// <summary>
    /// Gets the config page view model, if any is available
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the config page view model for</param>
    /// <returns>The config page view model of null if none is available</returns>
    public virtual GameOptionsDialog_ConfigPageViewModel? GetConfigPageViewModel(GameInstallation gameInstallation) => null;

    // TODO-14: Only return single item
    /// <summary>
    /// The progression game view models
    /// </summary>
    public virtual IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) =>
        Enumerable.Empty<ProgressionGameViewModel>();

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public virtual IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => Enumerable.Empty<GameFileLink>();

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public virtual IArchiveDataManager? GetArchiveDataManager(GameInstallation? gameInstallation) => null;

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public virtual FileSystemPath[]? GetArchiveFilePaths(FileSystemPath installDir) => null;

    /// <summary>
    /// Gets the utilities for this game
    /// </summary>
    /// <param name="gameInstallation">The game installation to use with the utilities</param>
    /// <returns>The utilities</returns>
    public virtual IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => Enumerable.Empty<Utility>();

    /// <summary>
    /// Gets the applied utilities for the specified game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the utilities for</param>
    /// <returns>The applied utilities</returns>
    public virtual Task<IList<string>> GetAppliedUtilitiesAsync(GameInstallation gameInstallation)
    {
        return Task.FromResult<IList<string>>(GetUtilities(gameInstallation).SelectMany(x => x.GetAppliedUtilities()).ToArray());
    }

    /// <summary>
    /// Gets the purchase links for the game
    /// </summary>
    public virtual IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => Enumerable.Empty<GamePurchaseLink>();

    /// <summary>
    /// Gets the additional overflow button items for the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the items for</param>
    public virtual IEnumerable<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems(GameInstallation gameInstallation) => 
        Enumerable.Empty<OverflowButtonItemViewModel>();

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public virtual GameFinder_GameItem? GetGameFinderItem() => null;

    /// <summary>
    /// Gets the info items for the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the info items for</param>
    /// <returns>The info items</returns>
    public virtual IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        VerifyGameInstallation(gameInstallation);

        return new[]
        {
            // TODO-14: Change this to show the platform
            //new DuoGridItemViewModel(
            //    header: new ResourceLocString(nameof(Resources.GameInfo_GameType)),
            //    text: GameTypeDisplayName,
            //    minUserLevel: UserLevel.Advanced),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_InstallDir)),
                text: gameInstallation.InstallLocation.FullPath),
            //new DuoGridItemViewModel("Install size", GameData.InstallDirectory.GetSize().ToString())
        };
    }

    /// <summary>
    /// Launches the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to launch</param>
    /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
    /// <returns>The task</returns>
    public async Task LaunchGameAsync(GameInstallation gameInstallation,
        // TODO-14: This should probably not be here since it's platform specific. Only Win32 has this.
        bool forceRunAsAdmin)
    {
        Logger.Trace("The game {0} is being launched...", Id);

        // Verify that the game can launch
        if (!await VerifyCanLaunchAsync(gameInstallation))
        {
            Logger.Info("The game {0} could not be launched", Id);
            return;
        }

        // Launch the game and get the process if available
        GameLaunchResult launchResult = await LaunchAsync(gameInstallation, forceRunAsAdmin);

        if (launchResult.SuccessfulLaunch)
            // Run any post launch operations on the process
            await PostLaunchAsync(launchResult.GameProcess);
    }

    /// <summary>
    /// Creates a shortcut to launch the game from
    /// </summary>
    /// <param name="gameInstallation">The game installation to create the shortcut for</param>
    /// <param name="shortcutName">The name of the shortcut</param>
    /// <param name="destinationDirectory">The destination directory for the shortcut</param>
    public abstract void CreateGameShortcut(GameInstallation gameInstallation, FileSystemPath shortcutName, FileSystemPath destinationDirectory);

    /// <summary>
    /// Gets the available jump list items for the game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the items for</param>
    /// <returns>The items</returns>
    public abstract IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation);

    /// <summary>
    /// Locates the game
    /// </summary>
    /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install location.</returns>
    public abstract Task<FileSystemPath?> LocateAsync();

    /// <summary>
    /// Gets the game installer info if available
    /// </summary>
    /// <returns>The game installer info or null if not available</returns>
    public virtual GameInstallerInfo? GetGameInstallerData() => null;

    /// <summary>
    /// Indicates if the game is valid
    /// </summary>
    /// <param name="installDir">The game install directory, if any</param>
    /// <returns>True if the game is valid, otherwise false</returns>
    public Task<bool> IsValidAsync(FileSystemPath installDir)
    {
        if (Services.Data.App_DisableGameValidation)
            return Task.FromResult(true);

        return IsGameLocationValidAsync(installDir);
    }

    /// <summary>
    /// Gets called as soon as the game is added
    /// </summary>
    /// <param name="gameInstallation">The added game installation</param>
    /// <returns>The task</returns>
    public virtual Task PostGameAddAsync(GameInstallation gameInstallation) => Task.CompletedTask;

    /// <summary>
    /// Gets called as soon as the game is removed
    /// </summary>
    /// <param name="gameInstallation">The game installation for the removed game</param>
    /// <returns>The task</returns>
    public virtual Task PostGameRemovedAsync(GameInstallation gameInstallation) => Task.CompletedTask;

    #endregion

    #region Data Types

    /// <summary>
    /// A game file link which can be accessed from the game
    /// </summary>
    public record GameFileLink(
        string Header, 
        FileSystemPath Path, 
        GenericIconKind Icon = GenericIconKind.None, 
        string? Arguments = null);

    /// <summary>
    /// A game purchase link which can be accessed from the game
    /// </summary>
    public record GamePurchaseLink(
        string Header,
        string Path, 
        GenericIconKind Icon = GenericIconKind.GameDisplay_Purchase);

    /// <summary>
    /// The result from launching a game
    /// </summary>
    protected record GameLaunchResult(
        Process? GameProcess, 
        bool SuccessfulLaunch);

    #endregion
}
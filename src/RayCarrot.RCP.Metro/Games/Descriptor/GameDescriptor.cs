#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.RCP.Metro.Archive;
using static RayCarrot.RCP.Metro.GameManager;

namespace RayCarrot.RCP.Metro;

// TODO-14: Clean up. Move code out of here. Rename to GameDescriptor? Move some things to modules/extensions. Use this instead of enum.
// TODO-14: Fix regions once refactoring is done as some properties are getting changed to methods.
// TODO-14: Rename some classes, such as demos, and remove any base classes. Everything should inherit from this directly and be sealed.

/// <summary>
/// The base for Rayman Control Panel game data
/// </summary>
public abstract class GameDescriptor
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
    public string IconSource => $"{AppViewModel.WPFApplicationBasePath}Img/GameIcons/{IconName}.png";

    #endregion

    #region Public Abstract Properties

    public abstract string Id { get; }
    public abstract Game Game { get; }
    public abstract GamePlatform Platform { get; }
    public abstract PlatformManager PlatformManager { get; }

    /// <summary>
    /// The game
    /// </summary>
    public abstract Games LegacyGame { get; } // TODO-14: Remove

    /// <summary>
    /// The game display name
    /// </summary>
    public abstract string DisplayName { get; } // TODO-14: Localize & split up into short and long name
    //public abstract LocalizedString ShortDisplayName { get; }
    //public abstract LocalizedString LongDisplayName { get; }

    /// <summary>
    /// The game backup name
    /// </summary>
    public virtual string BackupName => throw new InvalidOperationException($"The game {Id} has no backup name associated with it");

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
    protected virtual string IconName => $"{LegacyGame}";

    #endregion

    #region Public Virtual Properties

#nullable enable

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

#nullable disable

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
    public virtual IArchiveDataManager GetArchiveDataManager(
#nullable enable
        GameInstallation? gameInstallation) => null;
#nullable disable

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
        return Task.FromResult<IList<string>>(GetUtilities(gameInstallation).SelectMany(x => x.GetAppliedUtilities()).ToArray());
    }

    public virtual IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => Enumerable.Empty<Utility>();

    /// <summary>
    /// Gets the purchase links for the game
    /// </summary>
    public virtual IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => Enumerable.Empty<GamePurchaseLink>();

    #endregion

    #region Public Methods

    // TODO-14: Use this for all public APIs
    public void VerifyGameInstallation(GameInstallation gameInstallation)
    {
        if (gameInstallation == null) 
            throw new ArgumentNullException(nameof(gameInstallation));
        
        if (gameInstallation.Id != Id)
            throw new Exception($"The provided game installation ID {gameInstallation.Id} does not match {Id}");
    }

    #endregion

    #region Data Types

    /// <summary>
    /// A game file link which can be accessed from the game
    /// </summary>
    public record GameFileLink(
        string Header, 
        FileSystemPath Path, 
        GenericIconKind Icon = GenericIconKind.None, 
        string Arguments = null);

    #endregion
}
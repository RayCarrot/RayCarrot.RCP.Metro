using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

// TODO-14: Move descriptors to folders based on platform?
// TODO-14: Minimize the amount of methods here which do things by moving to manager classes retrieved through components.
//          The descriptor should really only be for providing data about the game and registered components.
// TODO-14: Consistent naming. Should 'game' be included in member names?

// It may be temping to have the games which inherit from GameDescriptor to be built up in a sort of hierarchy where they
// inherit from base classes, such as the 3 Fiesta Run game editions inheriting from a base Fiesta Run class, but we
// specifically do NOT want to do this. There are several reasons behind this which I won't go into here, but essentially
// I'm approaching this in more of a component-based way where common functionality and data should be relegated to
// separate classes which the descriptors then provide. In the future we might want to split this up further into more
// manager classes to the pointer where the platform base classes won't be needed.

/// <summary>
/// A game descriptor, providing data for a game
/// </summary>
public abstract class GameDescriptor : IComparable<GameDescriptor>
{
    #region Protected Constant Fields

    /// <summary>
    /// The group name to use for a dialog which requires reading/writing to a ubi.ini file
    /// </summary>
    protected const string UbiIniFileGroupName = "ubini-config"; // TODO-14: Remove from here

    #endregion

    #region Public Properties

    /// <summary>
    /// The game descriptor id. This should be unique and is used to identify this descriptor. Use this for logging as well.
    /// Note: There should never be hard-coded checks on this, which is why this is a string rather than an enum.
    /// </summary>
    public abstract string GameId { get; }

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
    public virtual LegacyGame? LegacyGame => null; // TODO-14: Minimize references to this

    /// <summary>
    /// The default game display name. This might be the same across multiple descriptors.
    /// For games with a short and long name then this will usually be the short one. For
    /// example "Rayman 2" is used instead of "Rayman 2: The Great Escape".
    /// </summary>
    public abstract LocalizedString DisplayName { get; } // TODO-UPDATE: Localize this for all games

    /// <summary>
    /// A unique display name for this game descriptor
    /// </summary>
    public virtual string GameDescriptorName => $"{DisplayName} ({Platform})"; // TODO-14: Implement this, localized, for each game and use where needed

    /// <summary>
    /// The game's release data. This is mainly used for sorting and doesn't have
    /// to be exact (i.e. January 1st is acceptable if only the year is known).
    /// </summary>
    public abstract DateTime ReleaseDate { get; }

    //public abstract LocalizedString ShortDisplayName { get; }
    //public abstract LocalizedString LongDisplayName { get; }

    /// <summary>
    /// Gets the default file name for launching the game, if available
    /// </summary>
    public abstract string DefaultFileName { get; } // TODO-14: Remove from here - not all games have an exe!

    /// <summary>
    /// The game icon asset
    /// </summary>
    public abstract GameIconAsset Icon { get; }

    /// <summary>
    /// The game banner asset
    /// </summary>
    public virtual GameBannerAsset Banner => GameBannerAsset.Default;

    /// <summary>
    /// The group names to use for the options, config and utility dialog
    /// </summary>
    public virtual IEnumerable<string> DialogGroupNames => Enumerable.Empty<string>(); // TODO-14: Change this

    /// <summary>
    /// Indicates if the game supports the game patcher
    /// </summary>
    public virtual bool AllowPatching => true;

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

    #endregion

    #region Protected Methods

    /// <summary>
    /// Registers the game components for a game installation
    /// </summary>
    /// <param name="builder">The component builder</param>
    protected virtual void RegisterComponents(GameComponentBuilder builder)
    {
        builder.Register<GameInfoComponent, DefaultGameInfoComponent>();
        builder.Register<GameValidationCheckComponent, InstallDataGameValidationCheckComponent>();
        builder.Register<OnGameRemovedComponent, RemoveFromJumpListOnGameRemovedComponent>();
        builder.Register<OnGameRemovedComponent, RemoveAddedFilesOnGameRemovedComponent>();

        // Give this low priority so that it runs last
        builder.Register<OnGameLaunchedComponent, OptionallyCloseAppOnGameLaunchedComponent>(ComponentPriority.Low);

        // Config page
        builder.Register(
            component: new GameOptionsDialogPageComponent(
                objFactory: x => x.GetRequiredComponent<GameConfigComponent>().CreateObject(),
                isAvailableFunc: x => x.HasComponent<GameConfigComponent>()),
            priority: ComponentPriority.High);

        // Utilities page
        builder.Register(
            component: new GameOptionsDialogPageComponent(
                objFactory: x => new UtilitiesPageViewModel(x.GetComponents<UtilityComponent>().
                    CreateObjects().
                    Select(utility => new UtilityViewModel(utility))),
                isAvailableFunc: x => x.HasComponent<UtilityComponent>()),
            priority: ComponentPriority.Low);
    }

    // TODO-14: Use this for all public APIs
    protected void VerifyGameInstallation(GameInstallation gameInstallation)
    {
        if (gameInstallation == null)
            throw new ArgumentNullException(nameof(gameInstallation));

        if (gameInstallation.GameId != GameId)
            throw new Exception($"The provided game id {gameInstallation.GameId} does not match {GameId}");
    }

    // TODO-14: Don't do this - an install location might be a file in case of roms
    /// <summary>
    /// Indicates if the game location is valid
    /// </summary>
    /// <param name="installLocation">The game install location</param>
    /// <returns>True if the game directory is valid, otherwise false</returns>
    protected virtual bool IsGameLocationValid(FileSystemPath installLocation)
    {
        // Make sure the default file exists in the install directory
        return (installLocation + DefaultFileName).FileExists;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Builds a new component provider for the game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to build the components for</param>
    /// <returns>The component provider</returns>
    public GameComponentProvider BuildComponents(GameInstallation gameInstallation)
    {
        // Create the builder
        GameComponentBuilder builder = new();

        // TODO-14: Ideally we'd pass in the game installation here so that which components
        //          get registered can depend on the game data, for example the progression
        //          managers for the edutainment games. The problem is this gets called before
        //          the game installation has been fully initialized if added. To solve that
        //          we either need to rebuild components twice in those cases or we have the
        //          OnGameAdded component be a virtual method in the GameDescriptor.
        // Register the components from the game descriptor
        RegisterComponents(builder);

        return builder.Build(gameInstallation);
    }

    public abstract IEnumerable<GameAddAction> GetAddActions();

    /// <summary>
    /// Gets the local uri links for the game. These are usually configuration program which come bundled with the game.
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the uri links for</param>
    /// <returns>The uri links</returns>
    public virtual IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => Enumerable.Empty<GameUriLink>();

    /// <summary>
    /// Gets the external uri links for the game. These are usually websites related to the game, such as the store page.
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the uri links for</param>
    /// <returns>The uri links</returns>
    public virtual IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => Enumerable.Empty<GameUriLink>();

    /// <summary>
    /// Gets optional RayMap map viewer information
    /// </summary>
    /// <returns>The info or null if not available</returns>
    public virtual RayMapInfo? GetRayMapInfo() => null; // TODO-UPDATE: Add for demos

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public virtual IArchiveDataManager? GetArchiveDataManager(GameInstallation? gameInstallation) => null;

    /// <summary>
    /// Gets the relative archive file paths for the game
    /// </summary>
    /// <param name="gameInstallation">The game installation, if available. This should only be used if absolutely needed.</param>
    public virtual IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => Enumerable.Empty<string>();

    // TODO-14: Probably remove or change this
    /// <summary>
    /// Gets the applied utilities for the specified game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the utilities for</param>
    /// <returns>The applied utilities</returns>
    public virtual Task<IList<string>> GetAppliedUtilitiesAsync(GameInstallation gameInstallation)
    {
        return Task.FromResult<IList<string>>(gameInstallation.GetComponents<UtilityComponent>().
            CreateObjects().
            SelectMany(x => x.GetAppliedUtilities()).
            ToArray());
    }

    /// <summary>
    /// Gets the purchase links for the game
    /// </summary>
    public virtual IEnumerable<GamePurchaseLink> GetPurchaseLinks() => Enumerable.Empty<GamePurchaseLink>();

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public virtual GameFinder_GameItem? GetGameFinderItem() => null;

    /// <summary>
    /// Indicates if the game is valid
    /// </summary>
    /// <param name="installDir">The game install directory, if any</param>
    /// <returns>True if the game is valid, otherwise false</returns>
    public bool IsValid(FileSystemPath installDir) => Services.Data.App_DisableGameValidation || IsGameLocationValid(installDir);

    public int CompareTo(GameDescriptor? other)
    {
        if (this == other)
            return 0;
        if (other == null)
            return 1;

        // Category
        int categoryComparison = Category.CompareTo(other.Category);
        if (categoryComparison != 0)
            return categoryComparison;

        // Game
        int gameComparison = Game.CompareTo(other.Game);
        if (gameComparison != 0)
            return gameComparison;

        // Platform
        int platformComparison = Platform.CompareTo(other.Platform);
        if (platformComparison != 0)
            return platformComparison;

        // Demo
        int demoComparison = IsDemo.CompareTo(other.IsDemo);
        if (demoComparison != 0)
            return demoComparison;

        // Release date
        int releaseDateComparison = ReleaseDate.CompareTo(other.ReleaseDate);
        if (releaseDateComparison != 0)
            return releaseDateComparison;

        // Id
        return String.Compare(GameId, other.GameId, StringComparison.Ordinal);
    }

    #endregion

    #region Data Types

    /// <summary>
    /// A game uri link for local and external locations
    /// </summary>
    /// <param name="Header">The link header</param>
    /// <param name="Uri">The link uri</param>
    /// <param name="Icon">An optional icon</param>
    /// <param name="Arguments">Optional arguments if it's a local file</param>
    public record GameUriLink(
        LocalizedString Header, 
        string Uri, 
        GenericIconKind Icon = GenericIconKind.None, 
        string? Arguments = null);

    public record RayMapInfo(
        RayMapViewer Viewer,
        string Mode,
        string Folder,
        string? Vol = null)
    {
        public string GetURL() => Viewer switch
        {
            RayMapViewer.RayMap => AppURLs.GetRayMapGameURL(Mode, Folder),
            RayMapViewer.Ray1Map => AppURLs.GetRay1MapGameURL(Mode, Folder, Vol),
            _ => throw new ArgumentOutOfRangeException(nameof(Viewer), Viewer, null)
        };

        public RayMapIconAsset GetIcon() => Viewer switch
        {
            RayMapViewer.RayMap => RayMapIconAsset.RayMap,
            RayMapViewer.Ray1Map => RayMapIconAsset.Ray1Map,
            _ => throw new ArgumentOutOfRangeException(nameof(Viewer), Viewer, null)
        };
    }

    public enum RayMapViewer { RayMap, Ray1Map }

    /// <summary>
    /// A game purchase link which can be accessed from the game
    /// </summary>
    public record GamePurchaseLink(
        LocalizedString Header,
        string Path, 
        GenericIconKind Icon = GenericIconKind.GameAdd_Purchase);

    #endregion
}
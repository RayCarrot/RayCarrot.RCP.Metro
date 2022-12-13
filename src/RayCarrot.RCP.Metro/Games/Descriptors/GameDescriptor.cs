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
    #region Constructor

    protected GameDescriptor()
    {
        ComponentProvider = BuildComponentProvider();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

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

    // TODO-14: Clean up how names are handled, split up like this?
    // GameDescriptorDisplayName: Unique name for the descriptor. For a specific game edition, for example "Rayman 2 (Steam)"
    // GameDisplayName: Normal game name, for example "Rayman 2"
    // FullGameDisplayName: Full game name, for example "Rayman 2: The Great Escape"
    // 
    // User can also define their own names. For example you might have multiple installations of Rayman 1 and they
    // can then be named differently.

    /// <summary>
    /// The game display name
    /// </summary>
    public abstract string DisplayName { get; } // TODO-14: Localize & split up into short and long name

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
    /// Indicates if the game should automatically be added to the jump list once added
    /// </summary>
    public virtual bool AutoAddToJumpList => !IsDemo;

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

    // TODO-14: Use this for all public APIs
    protected void VerifyGameInstallation(GameInstallation gameInstallation)
    {
        if (gameInstallation == null)
            throw new ArgumentNullException(nameof(gameInstallation));

        if (gameInstallation.GameId != GameId)
            throw new Exception($"The provided game id {gameInstallation.GameId} does not match {GameId}");
    }

    /// <summary>
    /// The implementation for launching the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to launch</param>
    /// <returns>True if the launch succeeded, otherwise false</returns>
    protected abstract Task<bool> LaunchAsync(GameInstallation gameInstallation);

    /// <summary>
    /// Post launch operations for the game which launched
    /// </summary>
    /// <returns>The task</returns>
    protected virtual async Task PostLaunchAsync()
    {
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
    protected virtual bool IsGameLocationValid(FileSystemPath installLocation)
    {
        // Make sure the default file exists in the install directory
        return (installLocation + DefaultFileName).FileExists;
    }

    #endregion

    #region Public Methods

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
        return Task.FromResult<IList<string>>(GetComponents<UtilityComponent>().
            CreateObjects(gameInstallation).
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
    /// Gets the info items for the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the info items for</param>
    /// <returns>The info items</returns>
    public virtual IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        VerifyGameInstallation(gameInstallation);

        return new[]
        {
            new DuoGridItemViewModel(
                header: "Game id:",
                text: GameId,
                minUserLevel: UserLevel.Debug),
            new DuoGridItemViewModel(
                header: "Installation id:",
                text: gameInstallation.InstallationId,
                minUserLevel: UserLevel.Debug),
            new DuoGridItemViewModel(
                header: "Components:",
                text: ComponentProvider.GetComponents().Select(x => x.GetType().Name).JoinItems(Environment.NewLine),
                minUserLevel: UserLevel.Debug),
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
    /// <returns>The task</returns>
    public async Task LaunchGameAsync(GameInstallation gameInstallation)
    {
        Logger.Trace("The game {0} is being launched...", GameId);

        // Launch the game
        bool success = await LaunchAsync(gameInstallation);

        if (success)
            // Run any post launch operations
            await PostLaunchAsync();
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
    /// Indicates if the game is valid
    /// </summary>
    /// <param name="installDir">The game install directory, if any</param>
    /// <returns>True if the game is valid, otherwise false</returns>
    public bool IsValid(FileSystemPath installDir) => Services.Data.App_DisableGameValidation || IsGameLocationValid(installDir);

    // TODO-14: Merge the install location check with this?
    /// <summary>
    /// Indicates if the game installation and its data is valid
    /// </summary>
    /// <param name="gameInstallation">The game installation to check</param>
    /// <returns>True if it's valid, otherwise false</returns>
    public virtual bool IsValid(GameInstallation gameInstallation) => 
        // The game is valid if every validation check returns true
        GetComponents<GameValidationCheckComponent>().All(x => x.IsValid(gameInstallation));

    // TODO-14: Rename to OnGameAddedAsync
    /// <summary>
    /// Gets called as soon as the game is added
    /// </summary>
    /// <param name="gameInstallation">The added game installation</param>
    /// <returns>The task</returns>
    public virtual Task PostGameAddAsync(GameInstallation gameInstallation) => Task.CompletedTask;

    // TODO-14: Rename to OnGameRemovedAsync
    /// <summary>
    /// Gets called as soon as the game is removed
    /// </summary>
    /// <param name="gameInstallation">The game installation for the removed game</param>
    /// <returns>The task</returns>
    public virtual Task PostGameRemovedAsync(GameInstallation gameInstallation)
    {
        AddedGameFiles? addedGameFiles = gameInstallation.GetObject<AddedGameFiles>(GameDataKey.RCP_AddedFiles);
        
        if (addedGameFiles == null)
            return Task.CompletedTask;

        foreach (FileSystemPath filePath in addedGameFiles.Files)
        {
            try
            {
                // Remove the file
                Services.File.DeleteFile(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Removing added game file");
            }
        }
        
        return Task.CompletedTask;
    }

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

    #region Components

    /// <summary>
    /// The provider for the descriptor components
    /// </summary>
    private DescriptorComponentProvider ComponentProvider { get; }

    /// <summary>
    /// Builds a new component provider for this descriptor
    /// </summary>
    /// <returns>The built provider</returns>
    private DescriptorComponentProvider BuildComponentProvider()
    {
        DescriptorComponentBuilder builder = new();
        RegisterComponents(builder);
        return builder.Build();
    }

    /// <summary>
    /// Registers the components for this descriptor. This will only be called once.
    /// </summary>
    /// <param name="builder">The component builder</param>
    protected virtual void RegisterComponents(DescriptorComponentBuilder builder)
    {
        builder.Register<GameValidationCheckComponent, InstallDataGameValidationCheckComponent>();
        
        // Config page
        builder.Register(new GameOptionsDialogPageComponent(
            objFactory: x => GetRequiredComponent<GameConfigComponent>().CreateObject(x),
            isAvailableFunc: _ => HasComponent<GameConfigComponent>(),
            priority: GameOptionsDialogPageComponent.PagePriority.High));

        // Utilities page
        builder.Register(new GameOptionsDialogPageComponent(
            objFactory: x => new UtilitiesPageViewModel(GetComponents<UtilityComponent>().
                CreateObjects(x).
                Select(utility => new UtilityViewModel(utility))),
            isAvailableFunc: _ => HasComponent<UtilityComponent>(),
            priority: GameOptionsDialogPageComponent.PagePriority.Low));
    }

    public bool HasComponent<T>() where T : DescriptorComponent => ComponentProvider.HasComponent<T>();
    public T? GetComponent<T>() where T : DescriptorComponent => ComponentProvider.GetComponent<T>();
    public T GetRequiredComponent<T>() where T : DescriptorComponent => ComponentProvider.GetRequiredComponent<T>();
    public IEnumerable<T> GetComponents<T>() where T : DescriptorComponent => ComponentProvider.GetComponents<T>();

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
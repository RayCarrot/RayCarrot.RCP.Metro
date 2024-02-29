using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

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
    #region Private Fields

    private ProgramInstallationStructure? _structure;

    #endregion

    #region Public Properties

    /// <summary>
    /// The game descriptor id. This should be unique and is used to identify this descriptor. Use this for logging as well.
    /// Note: There should never be hard-coded checks on this, which is why this is a string rather than an enum.
    /// </summary>
    public abstract string GameId { get; }

    /// <summary>
    /// The game id prior to version 14.0. Multiple games might share this id. This should only be used for
    /// backwards compatibility.
    /// </summary>
    public virtual string? LegacyGameId => null;

    /// <summary>
    /// The game this descriptor is for. This is mainly for categorization and filter purposes.
    /// </summary>
    public abstract Game Game { get; }

    /// <summary>
    /// The platform the descriptor is for. This is mainly for categorization and filter purposes.
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
    /// The default game display name. This might be the same across multiple descriptors.
    /// For games with a short and long name then this will usually be the short one. For
    /// example "Rayman 2" is used instead of "Rayman 2: The Great Escape".
    /// </summary>
    public abstract LocalizedString DisplayName { get; }

    /// <summary>
    /// Optional keywords to use when searching/filtering games. This should not include
    /// the display name.
    /// </summary>
    public virtual string[] SearchKeywords => Array.Empty<string>();

    /// <summary>
    /// The game's release data. This is mainly used for sorting and doesn't have
    /// to be exact (i.e. January 1st is acceptable if only the year is known).
    /// </summary>
    public abstract DateTime ReleaseDate { get; }

    /// <summary>
    /// The game icon asset
    /// </summary>
    public abstract GameIconAsset Icon { get; }

    /// <summary>
    /// The game banner asset
    /// </summary>
    public virtual GameBannerAsset Banner => GameBannerAsset.Default;

    /// <summary>
    /// Indicates if the game should default to use an available game client. This is mainly
    /// for games which require to be emulated as they can't launch by themselves.
    /// </summary>
    public virtual bool DefaultToUseGameClient => false;

    /// <summary>
    /// Gets the game structure for the game
    /// </summary>
    public ProgramInstallationStructure Structure => 
        // Cache the object to avoid re-creating it each time it's requested
        _structure ??= GetStructure();

    #endregion

    #region Protected Methods

    /// <summary>
    /// Registers the game components for a game installation
    /// </summary>
    /// <param name="builder">The component builder</param>
    protected virtual void RegisterComponents(IGameComponentBuilder builder)
    {
        builder.Register<GameInfoComponent, DefaultGameInfoComponent>();
        builder.Register<GameValidationCheckComponent, InstallDataGameValidationCheckComponent>();
        builder.Register<OnGameRemovedComponent, RemoveFromJumpListOnGameRemovedComponent>();
        builder.Register<OnGameRemovedComponent, RemoveAddedFilesOnGameRemovedComponent>();

        if (DefaultToUseGameClient)
            builder.Register<OnGameAddedComponent, AttachDefaultClientOnGameAddedComponent>();

        // Give this low priority so that it runs last
        builder.Register<OnGameLaunchedComponent, OptionallyCloseAppOnGameLaunchedComponent>(ComponentPriority.Low);
        builder.Register<OnGameLaunchedComponent, SetLastPlayedOnGameLaunchedComponent>(ComponentPriority.High);

        // For now we always show the game clients selection. Ideally it'd only show for some games, but that
        // in of itself would cause some inconsistencies. And only showing it when there are clients available
        // might be confusing for users who haven't added an emulator yet and can't find it in the ui.
        builder.Register(new GameOptionsComponent(x => new GameClientSelectionGameOptionsViewModel(x)));

        // Config page
        builder.Register(
            component: new GameOptionsDialogPageComponent(
                objFactory: x => x.GetRequiredComponent<GameConfigComponent>().CreateObject(),
                isAvailableFunc: x => x.HasComponent<GameConfigComponent>(),
                // Constant id since rebuilding components won't change this (we assume a game config
                // component does not come from a client)
                getInstanceIdFunc: _ => "GameConfig"),
            priority: ComponentPriority.High);

        // Utilities page
        builder.Register(
            component: new GameOptionsDialogPageComponent(
                objFactory: x => new UtilitiesPageViewModel(x.GetComponents<UtilityComponent>().
                    CreateObjects().
                    Select(utility => new UtilityViewModel(utility))),
                isAvailableFunc: x => x.HasComponent<UtilityComponent>(),
                // Constant id since rebuilding components won't change this (we assume a utility
                // component does not come from a client)
                getInstanceIdFunc: _ => "Utilities"),
            priority: ComponentPriority.Low);

        builder.Register<GamePanelComponent>(new ModLoaderGamePanelComponent());
    }

    protected abstract ProgramInstallationStructure GetStructure();

    #endregion

    #region Public Methods

    /// <summary>
    /// Registers the components without a game installation. This should only be used to analyze the components rather than use them.
    /// </summary>
    /// <returns>The component builder with the components ready to be built</returns>
    public GameComponentBuilder RegisterComponents()
    {
        // Create the builder
        GameComponentBuilder builder = new();

        // Register components from the structure
        Structure.RegisterComponents(builder);

        // Register the components from the game descriptor
        RegisterComponents(builder);

        return builder;
    }

    /// <summary>
    /// Registers the components for a game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to build the components for</param>
    /// <returns>The component builder with the components ready to be built</returns>
    public GameComponentBuilder RegisterComponents(GameInstallation gameInstallation)
    {
        // Create the builder
        GameComponentBuilder builder = new();

        // Ideally we'd pass in the game installation to the called methods so that
        // which components get registered can depend on the game data, for example
        // the progression managers for the edutainment games. The problem is this
        // gets called before the game installation has been fully initialized if
        // added. To solve that we either need to rebuild components twice in those
        // cases or we have the OnGameAdded component be a virtual method in the
        // GameDescriptor.

        // Register components from the structure
        Structure.RegisterComponents(builder);

        // Register the components from the game descriptor
        RegisterComponents(builder);

        // Register the components from an optional client. This can be emulators or game clients
        // such as Steam. When they register components they may override existing ones registered
        // by the game descriptor, thus changing some functionality for the game.
        GameClientInstallation? clientInstallation = Services.GameClients.GetAttachedGameClient(gameInstallation);
        clientInstallation?.GameClientDescriptor.RegisterComponents(builder);

        return builder;
    }

    public virtual IEnumerable<GameAddAction> GetAddActions() => Enumerable.Empty<GameAddAction>();

    /// <summary>
    /// Gets the purchase links for the game
    /// </summary>
    public virtual IEnumerable<GamePurchaseLink> GetPurchaseLinks() => Enumerable.Empty<GamePurchaseLink>();

    /// <summary>
    /// Gets the queries to use when finding the game
    /// </summary>
    /// <returns>The queries</returns>
    public virtual FinderQuery[] GetFinderQueries() => Array.Empty<FinderQuery>();

    /// <summary>
    /// Gets the finder item for this descriptor or null if there is none
    /// </summary>
    /// <returns>The finder item or null if there is none</returns>
    public GameFinderItem? GetFinderItem()
    {
        FinderQuery[] queries = GetFinderQueries();

        if (queries.Length == 0)
            return null;

        return new GameFinderItem(this, queries);
    }

    /// <summary>
    /// Gets a result indicating if the game location is valid
    /// </summary>
    /// <param name="installLocation">The game install location to check for</param>
    /// <returns>The validation result</returns>
    public GameLocationValidationResult ValidateLocation(InstallLocation installLocation)
    {
        // Always return true if game validation is disabled
        if (Services.Data.App_DisableGameValidation)
            return new GameLocationValidationResult(true);

        // Verify the game structure
        return Structure.IsLocationValid(installLocation);
    }

    public T GetStructure<T>()
        where T : ProgramInstallationStructure
    {
        if (Structure is not T structure)
            throw new InvalidOperationException($"The structure type {Structure.GetType()} for {GameId} does not match the requested type of {typeof(T)}");

        return structure;
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

    #region Data Types

    /// <summary>
    /// A game purchase link which can be accessed from the game
    /// </summary>
    public record GamePurchaseLink(
        LocalizedString Header,
        string Path, 
        GenericIconKind Icon = GenericIconKind.GameAdd_Purchase);

    #endregion
}
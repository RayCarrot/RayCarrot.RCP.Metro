using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public class GamesManager
{
    #region Constructor

    public GamesManager(AppUserData data, IMessenger messenger)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        // TODO-14: Remove Windows Package apps if OS is older than 8?
        // TODO-14: Reorder these here since demo category is removed. Order here shouldn't matter in the app though.
        GameDescriptors = new GameDescriptor[]
        {
            new GameDescriptor_Rayman1_MsDos(),
            new GameDescriptor_RaymanDesigner_MsDos(),
            new GameDescriptor_RaymanByHisFans_MsDos(),
            new GameDescriptor_Rayman60Levels_MsDos(),
            new GameDescriptor_Rayman2_Win32(),
            new GameDescriptor_RaymanM_Win32(),
            new GameDescriptor_RaymanArena_Win32(),
            new GameDescriptor_Rayman3_Win32(),
            new GameDescriptor_RaymanOrigins_Win32(),
            new GameDescriptor_RaymanLegends_Win32(),
            new GameDescriptor_RaymanJungleRun_WindowsPackage(),
            new GameDescriptor_RaymanFiestaRun_WindowsPackage(),
            new GameDescriptor_RaymanFiestaRun_PreloadEdition_WindowsPackage(),
            new GameDescriptor_RaymanFiestaRun_Windows10Edition_WindowsPackage(),

            new GameDescriptor_RaymanRavingRabbids_Win32(),
            new GameDescriptor_RaymanRavingRabbids2_Win32(),
            new GameDescriptor_RabbidsGoHome_Win32(),
            new GameDescriptor_RabbidsBigBang_WindowsPackage(),
            new GameDescriptor_RabbidsCoding_Win32(),

            new GameDescriptor_Rayman1_Demo_19951207_MsDos(),
            new GameDescriptor_Rayman1_Demo_19960215_MsDos(),
            new GameDescriptor_Rayman1_Demo_19951204_MsDos(),
            new GameDescriptor_RaymanGold_Demo_19970930_MsDos(),
            new GameDescriptor_Rayman2_Demo_19990818_Win32(),
            new GameDescriptor_Rayman2_Demo_19990904_Win32(),
            new GameDescriptor_RaymanM_Demo_20020627_Win32(),
            new GameDescriptor_Rayman3_Demo_20021004_Win32(),
            new GameDescriptor_Rayman3_Demo_20021021_Win32(),
            new GameDescriptor_Rayman3_Demo_20021210_Win32(),
            new GameDescriptor_Rayman3_Demo_20030129_Win32(),
            new GameDescriptor_Rayman3_Demo_20030108_Win32(),
            new GameDescriptor_RaymanRavingRabbids_Demo_20061106_Win32(),

            new GameDescriptor_Rayman1Minigames_Win32(),
            new GameDescriptor_RaymanEdutainmentEdu_MsDos(),
            new GameDescriptor_RaymanEdutainmentQuiz_MsDos(),
            new GameDescriptor_TonicTrouble_Win32(),
            new GameDescriptor_TonicTroubleSpecialEdition_Win32(),
            new GameDescriptor_RaymanDictées_Win32(),
            new GameDescriptor_RaymanPremiersClics_Win32(),
            new GameDescriptor_Rayman3PrintStudio_Win32(),
            new GameDescriptor_RaymanActivityCenter_Win32(),
            new GameDescriptor_RaymanRavingRabbidsActivityCenter_Win32(),

            new GameDescriptor_TheDarkMagiciansReignofTerror_Win32(),
            new GameDescriptor_RaymanRedemption_Win32(),
            new GameDescriptor_RaymanRedesigner_Win32(),
            new GameDescriptor_RaymanBowling2_Win32(),
            new GameDescriptor_RaymanGardenPLUS_Win32(),
            new GameDescriptor_GloboxMoment_Win32(),
        }.ToDictionary(x => x.GameId);
        SortedGameDescriptors = GameDescriptors.Values.OrderBy(x => x).ToArray();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Services

    private AppUserData Data { get; }
    private IMessenger Messenger { get; }

    #endregion

    #region Private Properties

    private Dictionary<string, GameDescriptor> GameDescriptors { get; }
    private GameDescriptor[] SortedGameDescriptors { get; }

    #endregion

    #region Private Methods

    private async Task<GameInstallation> AddGameImplAsync(
        GameDescriptor gameDescriptor, 
        FileSystemPath installDirectory,
        Action<GameInstallation>? configureInstallation = null)
    {
        Logger.Info("The game {0} is being added", gameDescriptor.GameId);

        // Create an installation
        GameInstallation gameInstallation = new(gameDescriptor, installDirectory);

        // Build the components
        gameInstallation.RebuildComponents();

        // Add the game
        Data.Game_GameInstallations.AddSorted(gameInstallation);

        try
        {
            // Invoke added actions
            await gameInstallation.GetComponents<OnGameAddedComponent>().InvokeAllAsync();

            // Configure
            configureInstallation?.Invoke(gameInstallation);

            Logger.Info("The game {0} has been added", gameInstallation.FullId);
        }
        catch
        {
            // Remove the game if there was an error during its initialization.
            // This is to avoid adding the game in an invalid state.
            Data.Game_GameInstallations.Remove(gameInstallation);
            throw;
        }

        return gameInstallation;
    }

    private async Task RemoveGameImplAsync(GameInstallation gameInstallation)
    {
        // Remove the game
        Data.Game_GameInstallations.Remove(gameInstallation);

        // Invoke removal actions
        await gameInstallation.GetComponents<OnGameRemovedComponent>().InvokeAllAsync();
    }

    #endregion

    #region Game Descriptor Methods

    /// <summary>
    /// Enumerates the available game descriptors
    /// </summary>
    /// <returns>The game descriptors</returns>
    public IReadOnlyList<GameDescriptor> GetGameDescriptors() => SortedGameDescriptors;

    /// <summary>
    /// Gets a game descriptor from the id
    /// </summary>
    /// <param name="gameId">The game descriptor id</param>
    /// <returns>The matching game descriptor</returns>
    public GameDescriptor GetGameDescriptor(string gameId)
    {
        if (gameId == null)
            throw new ArgumentNullException(nameof(gameId));

        if (!GameDescriptors.TryGetValue(gameId, out GameDescriptor descriptor))
            throw new ArgumentException($"No game descriptor found for the provided game id {gameId}", nameof(gameId));

        return descriptor;
    }

    /// <summary>
    /// Gets all of the game descriptors which match a legacy game id (a game id used prior to version 14.0)
    /// </summary>
    /// <param name="legacyGameId">The legacy game id</param>
    /// <returns>The matching game descriptors</returns>
    public IReadOnlyList<GameDescriptor> GetGameDescriptorsFromLegacyId(string legacyGameId)
    {
        if (legacyGameId == null) 
            throw new ArgumentNullException(nameof(legacyGameId));
        
        return SortedGameDescriptors.Where(x => x.LegacyGameId == legacyGameId).ToList();
    }

    #endregion

    #region Game Installation Methods

    /// <summary>
    /// Adds a new game to the app
    /// </summary>
    /// <param name="gameDescriptor">The game descriptor for the game to add</param>
    /// <param name="installDirectory">The game install directory</param>
    /// <param name="configureInstallation">An optional action callback for configuring the added game installation</param>
    /// <returns>The game installation</returns>
    public async Task<GameInstallation> AddGameAsync(
        GameDescriptor gameDescriptor, 
        FileSystemPath installDirectory, 
        Action<GameInstallation>? configureInstallation = null)
    {
        // Add the game
        GameInstallation gameInstallation = await AddGameImplAsync(gameDescriptor, installDirectory, configureInstallation);

        // Send a message that it's been added
        Messenger.Send(new AddedGamesMessage(gameInstallation));

        return gameInstallation;
    }

    /// <summary>
    /// Adds multiple new games to the app
    /// </summary>
    /// <param name="games">The games to add</param>
    /// <param name="configureInstallation">An optional action callback for configuring the added game installation</param>
    /// <returns>The game installations</returns>
    public async Task<IList<GameInstallation>> AddGamesAsync(
        IEnumerable<(GameDescriptor gameDescriptor, FileSystemPath installDirectory)> games,
        Action<GameInstallation>? configureInstallation = null)
    {
        List<GameInstallation> gameInstallations = new();

        // Add each game
        foreach ((GameDescriptor? gameDescriptor, FileSystemPath installDirectory) in games)
            gameInstallations.Add(await AddGameImplAsync(gameDescriptor, installDirectory, configureInstallation));

        if (gameInstallations.Any())
            Messenger.Send(new AddedGamesMessage(gameInstallations));

        return gameInstallations;
    }

    /// <summary>
    /// Removes the specified game from the app
    /// </summary>
    /// <param name="gameInstallation">The game installation to remove</param>
    /// <returns>The task</returns>
    public async Task RemoveGameAsync(GameInstallation gameInstallation)
    {
        // Remove the game
        await RemoveGameImplAsync(gameInstallation);

        // Send a message that the game was removed
        Messenger.Send(new RemovedGamesMessage(gameInstallation));
    }

    /// <summary>
    /// Removes multiple games from the app
    /// </summary>
    /// <param name="gameInstallations">The games to remove</param>
    /// <returns>The task</returns>
    public async Task RemoveGamesAsync(IList<GameInstallation> gameInstallations)
    {
        if (!gameInstallations.Any())
            return;

        // Remove the games
        foreach (GameInstallation gameInstallation in gameInstallations)
            await RemoveGameImplAsync(gameInstallation);

        Messenger.Send(new RemovedGamesMessage(gameInstallations));
    }

    /// <summary>
    /// Gets a collection of the currently installed games
    /// </summary>
    /// <returns>The game installations</returns>
    public IReadOnlyList<GameInstallation> GetInstalledGames()
    {
        // Copy to a list to avoid issues with it being modified when enumerating
        return Data.Game_GameInstallations.ToList();
    }

    /// <summary>
    /// Determines whether there are any installed games
    /// </summary>
    /// <returns>True if there is at least one installed game, otherwise false</returns>
    public bool AnyInstalledGames() => Data.Game_GameInstallations.Any();

    /// <summary>
    /// Determines whether there are any installed games which meet the specified criteria
    /// </summary>
    /// <returns>True if there is at least one installed game which meets the specified criteria, otherwise false</returns>
    public bool AnyInstalledGames(Func<GameInstallation, bool> predicate) => Data.Game_GameInstallations.Any(predicate);

    /// <summary>
    /// Finds a game installation based on the provided search predicate
    /// </summary>
    /// <param name="gameSearchPredicate">The predicate to use when finding the game installation</param>
    /// <returns>The first matching game installation or null if none was found</returns>
    public GameInstallation? FindInstalledGame(GameSearch.Predicate gameSearchPredicate) =>
        Data.Game_GameInstallations.
            Where(x => gameSearchPredicate(x.GameDescriptor)).
            OrderBy(x => x).
            FirstOrDefault();

    /// <summary>
    /// Finds a game installation based on the provided search predicates
    /// </summary>
    /// <param name="gameSearchPredicates">The predicates to use when finding the game installation. A game is valid if at least one matches.</param>
    /// <returns>The first matching game installation or null if none was found</returns>
    public GameInstallation? FindInstalledGame(params GameSearch.Predicate[] gameSearchPredicates) =>
        FindInstalledGame(GameSearch.Create(gameSearchPredicates));

    /// <summary>
    /// Gets a game installation from the installation id
    /// </summary>
    /// <param name="installationId">The game installation id</param>
    /// <returns>The matching game installation or null if not found</returns>
    public GameInstallation? GetInstalledGame(string installationId)
    {
        if (installationId == null)
            throw new ArgumentNullException(nameof(installationId));

        return Data.Game_GameInstallations.FirstOrDefault(x => x.InstallationId == installationId);
    }

    #endregion
}
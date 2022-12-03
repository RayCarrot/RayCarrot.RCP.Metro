using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using NLog;

namespace RayCarrot.RCP.Metro;

// TODO-14: Make this thread-safe
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
            new GameDescriptor_Rayman1_MSDOS(),
            new GameDescriptor_RaymanDesigner_MSDOS(),
            new GameDescriptor_RaymanByHisFans_MSDOS(),
            new GameDescriptor_Rayman60Levels_MSDOS(),
            new GameDescriptor_Rayman2_Win32(),
            new GameDescriptor_Rayman2_Steam(),
            new GameDescriptor_RaymanM_Win32(),
            new GameDescriptor_RaymanArena_Win32(),
            new GameDescriptor_Rayman3_Win32(),
            new GameDescriptor_RaymanOrigins_Win32(),
            new GameDescriptor_RaymanOrigins_Steam(),
            new GameDescriptor_RaymanLegends_Win32(),
            new GameDescriptor_RaymanLegends_Steam(),
            new GameDescriptor_RaymanJungleRun_WindowsPackage(),
            new GameDescriptor_RaymanFiestaRun_WindowsPackage(),
            new GameDescriptor_RaymanFiestaRun_PreloadEdition_WindowsPackage(),
            new GameDescriptor_RaymanFiestaRun_Windows10Edition_WindowsPackage(),

            new GameDescriptor_RaymanRavingRabbids_Win32(),
            new GameDescriptor_RaymanRavingRabbids_Steam(),
            new GameDescriptor_RaymanRavingRabbids2_Win32(),
            new GameDescriptor_RabbidsGoHome_Win32(),
            new GameDescriptor_RabbidsBigBang_WindowsPackage(),
            new GameDescriptor_RabbidsCoding_Win32(),

            new GameDescriptor_Rayman1_Demo_19951207_MSDOS(),
            new GameDescriptor_Rayman1_Demo_19960215_MSDOS(),
            new GameDescriptor_Rayman1_Demo_19951204_MSDOS(),
            new GameDescriptor_RaymanGold_Demo_19970930_MSDOS(),
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
            new GameDescriptor_RaymanEdutainmentEdu_MSDOS(),
            new GameDescriptor_RaymanEdutainmentQuiz_MSDOS(),
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
        }.ToDictionary(x => x.Id);

        EmulatorDescriptors = new EmulatorDescriptor[]
        {
            new DOSBoxEmulatorDescriptor(),
        }.ToDictionary(x => x.EmulatorId);
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
    private Dictionary<string, EmulatorDescriptor> EmulatorDescriptors { get; }

    #endregion

    #region Private Methods

    private async Task<GameInstallation> AddGameImplAsync(
        GameDescriptor gameDescriptor, 
        FileSystemPath installDirectory,
        Action<GameInstallation>? configureInstallation = null)
    {
        Logger.Info("The game {0} is being added", gameDescriptor.Id);

        // TODO-14: Remove this
        // Make sure the game hasn't already been added
        //if (gameDescriptor.LegacyGame.IsAdded())
        //{
        //    Logger.Warn("The game {0} has already been added", gameDescriptor.Id);

        //    await MessageUI.DisplayMessageAsync(String.Format(Resources.AddGame_Duplicate, gameDescriptor.DisplayName), 
        //        Resources.AddGame_DuplicateHeader, MessageType.Error);

        //    return null;
        //}

        // Create an installation
        GameInstallation gameInstallation = new(gameDescriptor, installDirectory);

        // Add the game
        Data.Game_GameInstallations.Add(gameInstallation);

        Logger.Info("The game {0} has been added", gameInstallation.Id);

        // Run post-add operations
        await gameDescriptor.PostGameAddAsync(gameInstallation);

        // Add the game to the jump list
        if (gameDescriptor.AutoAddToJumpList)
            Data.App_JumpListItemIDCollection.AddRange(gameDescriptor.GetJumpListItems(gameInstallation).Select(x => x.ID));

        // Configure
        configureInstallation?.Invoke(gameInstallation);
        
        return gameInstallation;
    }

    private async Task RemoveGameImplAsync(GameInstallation gameInstallation)
    {
        try
        {
            // Remove the game from the jump list
            foreach (JumpListItemViewModel item in gameInstallation.GameDescriptor.GetJumpListItems(gameInstallation))
                Data.App_JumpListItemIDCollection?.RemoveWhere(x => x == item.ID);

            // Remove the game
            Data.Game_GameInstallations.Remove(gameInstallation);

            // Run post game removal
            await gameInstallation.GameDescriptor.PostGameRemovedAsync(gameInstallation);
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Removing game");
            throw;
        }
    }

    #endregion

    #region Public Game Methods

    /// <summary>
    /// Adds a new game to the app data
    /// </summary>
    /// <param name="gameDescriptor">The game descriptor for the game to add</param>
    /// <param name="installDirectory">The game install directory</param>
    /// <returns>The game installation</returns>
    public async Task<GameInstallation> AddGameAsync(
        GameDescriptor gameDescriptor, 
        FileSystemPath installDirectory, 
        Action<GameInstallation>? configureInstallation = null)
    {
        GameInstallation gameInstallation = await AddGameImplAsync(gameDescriptor, installDirectory, configureInstallation);

        Messenger.Send(new AddedGamesMessage(gameInstallation));

        return gameInstallation;
    }

    public async Task<IList<GameInstallation>> AddGamesAsync(IEnumerable<(GameDescriptor gameDescriptor, FileSystemPath installDirectory)> games)
    {
        List<GameInstallation> gameInstallations = new();

        foreach (var game in games)
            gameInstallations.Add(await AddGameImplAsync(game.gameDescriptor, game.installDirectory));

        Messenger.Send(new AddedGamesMessage(gameInstallations));

        return gameInstallations;
    }

    /// <summary>
    /// Removes the specified game
    /// </summary>
    /// <param name="gameInstallation">The game installation to remove</param>
    /// <param name="forceRemove">Indicates if the game should be force removed</param>
    /// <returns>The task</returns>
    public async Task RemoveGameAsync(GameInstallation gameInstallation)
    {
        await RemoveGameImplAsync(gameInstallation);
        Messenger.Send(new RemovedGamesMessage(gameInstallation));
    }

    public async Task RemoveGamesAsync(IList<GameInstallation> gameInstallations)
    {
        foreach (GameInstallation gameInstallation in gameInstallations)
            await RemoveGameImplAsync(gameInstallation);

        Messenger.Send(new RemovedGamesMessage(gameInstallations));
    }

    // TODO-14: Should we sort these? The order here should not be assumed. Check for game add dialog how it's handled.
    /// <summary>
    /// Enumerates the installed games
    /// </summary>
    /// <returns>The game installations</returns>
    public IEnumerable<GameInstallation> EnumerateInstalledGames() => Data.Game_GameInstallations;

    /// <summary>
    /// Enumerates the installed games which have the specified id
    /// </summary>
    /// <param name="id">The id to check for</param>
    /// <returns>The installed games which have the id</returns>
    public IEnumerable<GameInstallation> EnumerateInstalledGames(string id) => Data.Game_GameInstallations.Where(x => x.Id == id);

    /// <summary>
    /// Finds a game installation based on the provided search predicate
    /// </summary>
    /// <param name="gameSearchPredicate">The predicate to use when finding the game installation</param>
    /// <returns>The first matching game installation or null if none was found</returns>
    public GameInstallation? FindGameInstallation(GameSearch.Predicate gameSearchPredicate) =>
        EnumerateInstalledGames().FirstOrDefault(x => gameSearchPredicate(x.GameDescriptor));

    /// <summary>
    /// Finds a game installation based on the provided search predicates
    /// </summary>
    /// <param name="gameSearchPredicates">The predicates to use when finding the game installation. A game is valid if at least one matches.</param>
    /// <returns>The first matching game installation or null if none was found</returns>
    public GameInstallation? FindGameInstallation(params GameSearch.Predicate[] gameSearchPredicates) =>
        FindGameInstallation(GameSearch.Create(gameSearchPredicates));

    /// <summary>
    /// Enumerates the available game descriptors
    /// </summary>
    /// <returns>The game descriptors</returns>
    public IEnumerable<GameDescriptor> EnumerateGameDescriptors() => GameDescriptors.Values;

    /// <summary>
    /// Gets a game descriptor from the id
    /// </summary>
    /// <param name="id">The game descriptor id</param>
    /// <returns>The matching game descriptor</returns>
    public GameDescriptor GetGameDescriptor(string id)
    {
        if (id == null) 
            throw new ArgumentNullException(nameof(id));
        
        // TODO-14: Add check and throw if not found?
        return GameDescriptors[id];
    }

    #endregion

    #region Public Emulator Methods

    /// <summary>
    /// Gets an emulator descriptor from the id
    /// </summary>
    /// <param name="emulatorId">The emulator id</param>
    /// <returns>The matching emulator descriptor</returns>
    public EmulatorDescriptor GetEmulatorDescriptor(string emulatorId)
    {
        if (emulatorId == null)
            throw new ArgumentNullException(nameof(emulatorId));

        // TODO-14: Add check and throw if not found?
        return EmulatorDescriptors[emulatorId];
    }

    /// <summary>
    /// Gets an emulator installation from the installation id
    /// </summary>
    /// <param name="installationId">The emulator installation id</param>
    /// <returns>The matching emulator installation or null if not found</returns>
    public EmulatorInstallation? GetEmulatorInstallation(string installationId)
    {
        if (installationId == null)
            throw new ArgumentNullException(nameof(installationId));

        return Data.Game_EmulatorInstallations.FirstOrDefault(x => x.InstallationId == installationId);
    }

    #endregion
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinarySerializer;
using NLog;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class GamesManager
{
    #region Constructor

    public GamesManager(AppUserData data, IMessageUIManager messageUi)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));

        GameDescriptors = new GameDescriptor[]
        {
            new GameDescriptor_Rayman1(),
            new GameDescriptor_RaymanDesigner(),
            new GameDescriptor_RaymanByHisFans(),
            new GameDescriptor_Rayman60Levels(),
            new GameDescriptor_Rayman2(),
            new GameDescriptor_RaymanM(),
            new GameDescriptor_RaymanArena(),
            new GameDescriptor_Rayman3(),
            new GameDescriptor_RaymanOrigins(),
            new GameDescriptor_RaymanLegends(),
            new GameDescriptor_RaymanJungleRun(),
            new GameDescriptor_RaymanFiestaRun(),

            new GameDescriptor_RaymanRavingRabbids(),
            new GameDescriptor_RaymanRavingRabbids2(),
            new GameDescriptor_RabbidsGoHome(),
            new GameDescriptor_RabbidsBigBang(),
            new GameDescriptor_RabbidsCoding(),

            new GameDescriptor_Rayman1Demo1(),
            new GameDescriptor_Rayman1Demo2(),
            new GameDescriptor_Rayman1Demo3(),
            new GameDescriptor_RaymanGoldDemo(),
            new GameDescriptor_Rayman2Demo1(),
            new GameDescriptor_Rayman2Demo2(),
            new GameDescriptor_RaymanMDemo(),
            new GameDescriptor_Rayman3Demo1(),
            new GameDescriptor_Rayman3Demo2(),
            new GameDescriptor_Rayman3Demo3(),
            new GameDescriptor_Rayman3Demo4(),
            new GameDescriptor_Rayman3Demo5(),
            new GameDescriptor_RaymanRavingRabbidsDemo(),

            new GameDescriptor_Ray1Minigames(),
            new GameDescriptor_EducationalDos(),
            new GameDescriptor_TonicTrouble(),
            new GameDescriptor_TonicTroubleSpecialEdition(),
            new GameDescriptor_RaymanDictées(),
            new GameDescriptor_RaymanPremiersClics(),
            new GameDescriptor_PrintStudio(),
            new GameDescriptor_RaymanActivityCenter(),
            new GameDescriptor_RaymanRavingRabbidsActivityCenter(),

            new GameDescriptor_TheDarkMagiciansReignofTerror(),
            new GameDescriptor_RaymanRedemption(),
            new GameDescriptor_RaymanRedesigner(),
            new GameDescriptor_RaymanBowling2(),
            new GameDescriptor_RaymanGardenPLUS(),
            new GameDescriptor_GloboxMoment(),
        }.ToDictionary(x => x.Id);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Services

    private AppUserData Data { get; }
    private IMessageUIManager MessageUI { get; } // TODO-14: Remove need for this

    #endregion

    #region Private Properties

    /// <summary>
    /// The available game infos
    /// </summary>
    private Dictionary<string, GameDescriptor> GameDescriptors { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a new game to the app data
    /// </summary>
    /// <param name="game">The game to add</param>
    /// <param name="type">The game type</param>
    /// <param name="installDirectory">The game install directory</param>
    /// <param name="isRCPInstalled">Indicates if the game was installed through the Rayman Control Panel</param>
    /// <returns>The game installation</returns>
    public async Task<GameInstallation?> AddGameAsync(Games game, GameType type, FileSystemPath installDirectory, bool isRCPInstalled)
    {
        Logger.Info("The game {0} is being added of type {1}...", game, type);

        // TODO-14: Remove this check
        // Make sure the game hasn't already been added
        if (game.IsAdded())
        {
            Logger.Warn("The game {0} has already been added", game);

            await MessageUI.DisplayMessageAsync(String.Format(Resources.AddGame_Duplicate, game), 
                Resources.AddGame_DuplicateHeader, MessageType.Error);

            return null;
        }

        // Create an installation
        GameInstallation gameInstallation = new(game, type, installDirectory, isRCPInstalled);

        // Get the manager
        GameManager manager = gameInstallation.Game.GetManager(type);

        // Add the game
        Data.Game_GameInstallations.Add(gameInstallation);

        Logger.Info("The game {0} has been added", gameInstallation.Id);

        // Run post-add operations
        await manager.PostGameAddAsync(gameInstallation);

        // Add the game to the jump list
        if (gameInstallation.GameDescriptor.AutoAddToJumpList)
            Data.App_JumpListItemIDCollection.AddRange(manager.GetJumpListItems(gameInstallation).Select(x => x.ID));

        return gameInstallation;
    }

    /// <summary>
    /// Removes the specified game
    /// </summary>
    /// <param name="gameInstallation">The game installation to remove</param>
    /// <param name="forceRemove">Indicates if the game should be force removed</param>
    /// <returns>The task</returns>
    public async Task RemoveGameAsync(GameInstallation gameInstallation, bool forceRemove)
    {
        try
        {
            // Get the manager
            GameManager manager = gameInstallation.Game.GetManager();

            // TODO-14: Move this out of here
            if (!forceRemove)
            {
                // Get applied utilities
                IList<string> appliedUtilities = await gameInstallation.GameDescriptor.GetAppliedUtilitiesAsync(gameInstallation);

                // Warn about applied utilities, if any
                if (appliedUtilities.Any() && !await MessageUI.DisplayMessageAsync(
                        $"{Resources.RemoveGame_UtilityWarning}{Environment.NewLine}{Environment.NewLine}" +
                        $"{appliedUtilities.JoinItems(Environment.NewLine)}", 
                        Resources.RemoveGame_UtilityWarningHeader, MessageType.Warning, true))
                    return;

                // Get applied patches
                using Context context = new RCPContext(String.Empty);
                PatchLibrary library = new(gameInstallation.InstallLocation, Services.File);
                PatchLibraryFile? libraryFile = null;

                try
                {
                    libraryFile = context.ReadFileData<PatchLibraryFile>(library.LibraryFilePath);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Reading patch library");
                }

                // Warn about applied patches, if any
                if (libraryFile?.Patches.Any(x => x.IsEnabled) == true && !await MessageUI.DisplayMessageAsync(String.Format(Resources.RemoveGame_PatchWarning, libraryFile.Patches.Count(x => x.IsEnabled)), MessageType.Warning, true))
                    return;
            }

            // Remove the game from the jump list
            foreach (JumpListItemViewModel item in manager.GetJumpListItems(gameInstallation))
                Data.App_JumpListItemIDCollection?.RemoveWhere(x => x == item.ID);

            // Remove the game
            Data.Game_GameInstallations.Remove(gameInstallation);

            // Run post game removal
            await manager.PostGameRemovedAsync();
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Removing game");
            throw;
        }
    }

    /// <summary>
    /// Enumerates the installed games
    /// </summary>
    /// <returns>The game installations</returns>
    public IEnumerable<GameInstallation> EnumerateInstalledGames() => Data.Game_GameInstallations;

    /// <summary>
    /// Enumerates the available game descriptors
    /// </summary>
    /// <returns>The game descriptors</returns>
    public IEnumerable<GameDescriptor> EnumerateGameDescriptors() => GameDescriptors.Values;

    public GameDescriptor GetGameDescriptor(string id)
    {
        // TODO-14: Add check and throw if not found?
        return GameDescriptors[id];
    }

    #endregion
}
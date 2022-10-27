﻿using System;
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

            new GameDescriptor_Ray1Minigames_Win32(),
            new GameDescriptor_EducationalDos_MSDOS(),
            new GameDescriptor_TonicTrouble_Win32(),
            new GameDescriptor_TonicTroubleSpecialEdition_Win32(),
            new GameDescriptor_RaymanDictées_Win32(),
            new GameDescriptor_RaymanPremiersClics_Win32(),
            new GameDescriptor_PrintStudio_Win32(),
            new GameDescriptor_RaymanActivityCenter_Win32(),
            new GameDescriptor_RaymanRavingRabbidsActivityCenter_Win32(),

            new GameDescriptor_TheDarkMagiciansReignofTerror_Win32(),
            new GameDescriptor_RaymanRedemption_Win32(),
            new GameDescriptor_RaymanRedesigner_Win32(),
            new GameDescriptor_RaymanBowling2_Win32(),
            new GameDescriptor_RaymanGardenPLUS_Win32(),
            new GameDescriptor_GloboxMoment_Win32(),
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
    /// <param name="gameDescriptor">The game descriptor for the game to add</param>
    /// <param name="type">The game type</param>
    /// <param name="installDirectory">The game install directory</param>
    /// <param name="isRCPInstalled">Indicates if the game was installed through the Rayman Control Panel</param>
    /// <returns>The game installation</returns>
    public async Task<GameInstallation> AddGameAsync(GameDescriptor gameDescriptor, FileSystemPath installDirectory, bool isRCPInstalled)
    {
        Logger.Info("The game {0} is being added", gameDescriptor.Id);

        // TODO-14: Remove this
        // Make sure the game hasn't already been added
        if (gameDescriptor.LegacyGame.IsAdded())
        {
            Logger.Warn("The game {0} has already been added", gameDescriptor.Id);

            await MessageUI.DisplayMessageAsync(String.Format(Resources.AddGame_Duplicate, gameDescriptor.DisplayName), 
                Resources.AddGame_DuplicateHeader, MessageType.Error);

            return null;
        }

        // Create an installation
        GameInstallation gameInstallation = new(gameDescriptor.Id, installDirectory, isRCPInstalled);

        // Get the manager
        GameManager manager = gameInstallation.GameManager;

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
            GameManager manager = gameInstallation.GameManager;

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
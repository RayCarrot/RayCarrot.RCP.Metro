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
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Services

    private AppUserData Data { get; }
    private IMessageUIManager MessageUI { get; } // TODO-14: Remove need for this

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a new game to the app data
    /// </summary>
    /// <param name="game">The game to add</param>
    /// <param name="type">The game type</param>
    /// <param name="installDirectory">The game install directory</param>
    /// <returns>The game installation</returns>
    public async Task<GameInstallation?> AddGameAsync(Games game, GameType type, FileSystemPath installDirectory)
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
        GameInstallation gameInstallation = new(game, type, installDirectory);

        // Get the manager
        GameManager manager = gameInstallation.Game.GetManager(type);

        // Add the game
        Data.Game_GameInstallations.Add(gameInstallation);

        Logger.Info("The game {0} has been added", gameInstallation.ID);

        // Run post-add operations
        await manager.PostGameAddAsync(gameInstallation);

        // Add the game to the jump list
        if (gameInstallation.GameInfo.AutoAddToJumpList)
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
                IList<string> appliedUtilities = await gameInstallation.GameInfo.GetAppliedUtilitiesAsync(gameInstallation);

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

            // Remove game from installed games if it was installed
            Data.Game_InstalledGames.Remove(gameInstallation.Game);

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

    #endregion
}
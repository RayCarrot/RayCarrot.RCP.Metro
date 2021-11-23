using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;
using NLog;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Base view model for synchronizing texture info for a game
/// </summary>
public class Utility_BaseGameSyncTextureInfo_ViewModel : Utility_BaseSyncTextureInfoViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="game">The game</param>
    /// <param name="gameMode">The game mode</param>
    /// <param name="gameDataDirNames">The game data directory names</param>
    public Utility_BaseGameSyncTextureInfo_ViewModel(Games game, GameMode gameMode, string[] gameDataDirNames)
    {
        Game = game;
        GameMode = gameMode;
        GameDataDirNames = gameDataDirNames;
        // Create commands
        CorrectTextureInfoCommand = new AsyncRelayCommand(SyncTextureInfoAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Protected Abstract Properties

    /// <summary>
    /// The game
    /// </summary>
    protected Games Game { get; }

    /// <summary>
    /// The game mode
    /// </summary>
    protected GameMode GameMode { get; }

    /// <summary>
    /// The game data directory names
    /// </summary>
    protected string[] GameDataDirNames { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the utility is loading
    /// </summary>
    public bool IsLoading { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Synchronizes the texture info for the selected game data directory
    /// </summary>
    /// <returns>The task</returns>
    public async Task SyncTextureInfoAsync()
    {
        try
        {
            IsLoading = true;

            var syncResult = await Task.Run(() =>
            {
                // Get the game install directory
                var installDir = Game.GetInstallDir();

                // Get the settings
                var attr = GameMode.GetAttribute<OpenSpaceGameModeInfoAttribute>();
                var gameSettings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

                // Get the file extension for the level data files
                var fileExt = GetLevelFileExtension(gameSettings);

                // Get the level data files
                var dataFiles = GameDataDirNames.Select(x => Directory.GetFiles(installDir + x, $"*{fileExt}", SearchOption.AllDirectories).Select(y => new FileSystemPath(y))).SelectMany(x => x);

                // Get the .cnt file names
                var fileNames = GetCntFileNames(gameSettings);

                // Get the full paths and only keep the ones which exist
                var cntFiles = GameDataDirNames.Select(dataDir => fileNames.Select(cnt => installDir + dataDir + cnt).Where(cntPath => cntPath.FileExists)).SelectMany(x => x);

                // Sync the texture info
                return EditTextureInfo(gameSettings, dataFiles, cntFiles);
            });

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Utilities_SyncTextureInfo_Success, syncResult.EditedTextures, syncResult.TotalTextures));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Syncing texture info");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_SyncTextureInfo_Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Commands

    public ICommand CorrectTextureInfoCommand { get; }

    #endregion
}
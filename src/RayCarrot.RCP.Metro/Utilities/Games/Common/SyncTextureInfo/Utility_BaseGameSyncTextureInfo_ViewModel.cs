using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer.OpenSpace;
using NLog;

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
    public Utility_BaseGameSyncTextureInfo_ViewModel(Games game, CPAGameMode gameMode, string[] gameDataDirNames)
    {
        // Set properties
        Game = game;
        GameMode = gameMode;
        GameDataDirNames = gameDataDirNames;

        // Create commands
        SyncTextureInfoCommand = new AsyncRelayCommand(() => SyncTextureInfoAsync());
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand SyncTextureInfoCommand { get; }

    #endregion

    #region Protected Abstract Properties

    /// <summary>
    /// The game
    /// </summary>
    protected Games Game { get; }

    /// <summary>
    /// The game mode
    /// </summary>
    protected CPAGameMode GameMode { get; }

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
    /// <param name="cntFiles">The .cnt file paths, or null if they should be automatically found</param>
    /// <returns>The task</returns>
    public async Task SyncTextureInfoAsync(IEnumerable<FileSystemPath>? cntFiles = null)
    {
        try
        {
            IsLoading = true;

            TextureInfoEditResult syncResult = await Task.Run(() =>
            {
                // Get the game install directory
                FileSystemPath installDir = Game.GetInstallDir();

                // Get the settings
                CPAGameModeInfoAttribute attr = GameMode.GetAttribute<CPAGameModeInfoAttribute>();
                OpenSpaceSettings gameSettings = attr.GetSettings();

                // Get the file extension for the level data files
                string fileExt = GetLevelFileExtension(gameSettings);

                // Get the level data files
                IEnumerable<FileSystemPath> dataFiles = GameDataDirNames.
                    Select(x => Directory.GetFiles(installDir + x, $"*{fileExt}", SearchOption.AllDirectories).
                        Select(y => new FileSystemPath(y))).
                    SelectMany(x => x);

                if (cntFiles == null)
                {
                    // Get the .cnt file names
                    string[] fileNames = GetCntFileNames(gameSettings);

                    // Get the full paths and only keep the ones which exist
                    cntFiles = GameDataDirNames.
                        Select(dataDir => fileNames.
                            Select(cnt => installDir + dataDir + cnt).
                            Where(cntPath => cntPath.FileExists)).
                        SelectMany(x => x);
                }

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
}
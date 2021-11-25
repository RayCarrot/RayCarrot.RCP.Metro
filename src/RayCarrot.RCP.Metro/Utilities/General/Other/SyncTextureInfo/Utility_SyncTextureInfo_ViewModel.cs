#nullable disable
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for synchronizing texture info
/// </summary>
public class Utility_SyncTextureInfo_ViewModel : Utility_BaseSyncTextureInfoViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_SyncTextureInfo_ViewModel()
    {
        // Create commands
        CorrectTextureInfoCommand = new AsyncRelayCommand(SyncTextureInfoAsync);

        // Set up selection
        GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.Rayman2PC, new GameMode[]
        {
            GameMode.Rayman2PC,
            GameMode.RaymanMPC,
            GameMode.RaymanArenaPC,
            GameMode.Rayman3PC,
            GameMode.TonicTroublePC,
            GameMode.TonicTroubleSEPC,
        });
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The game mode selection
    /// </summary>
    public EnumSelectionViewModel<GameMode> GameModeSelection { get; }

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
        var result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
        {
            Title = Resources.Utilities_SyncTextureInfo_SelectDirHeader,
            DefaultDirectory = GameModeSelection.SelectedValue.GetGame()?.GetInstallDir(false) ?? FileSystemPath.EmptyPath
        });

        if (result.CanceledByUser)
            return;

        try
        {
            IsLoading = true;

            var syncResult = await Task.Run(() =>
            {
                // Get the settings
                var attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();
                var gameSettings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

                // Get the file extension for the level data files
                var fileExt = GetLevelFileExtension(gameSettings);

                // Get the level data files
                var dataFiles = Directory.GetFiles(result.SelectedDirectory, $"*{fileExt}", SearchOption.AllDirectories).Select(x => new FileSystemPath(x));

                // Get the .cnt file names
                var fileNames = GetCntFileNames(gameSettings);

                // Get the full paths and only keep the ones which exist
                var cntFiles = fileNames.Select(x => result.SelectedDirectory + x).Where(x => x.FileExists);

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
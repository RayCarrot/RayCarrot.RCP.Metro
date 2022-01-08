using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer.OpenSpace;
using NLog;

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
        SyncTextureInfoCommand = new AsyncRelayCommand(SyncTextureInfoAsync);

        // Set up selection
        GameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2_PC, new OpenSpaceGameMode[]
        {
            OpenSpaceGameMode.Rayman2_PC,
            OpenSpaceGameMode.RaymanM_PC,
            OpenSpaceGameMode.RaymanArena_PC,
            OpenSpaceGameMode.Rayman3_PC,
            OpenSpaceGameMode.TonicTrouble_PC,
            OpenSpaceGameMode.TonicTrouble_SE_PC,
        });
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand SyncTextureInfoCommand { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The game mode selection
    /// </summary>
    public EnumSelectionViewModel<OpenSpaceGameMode> GameModeSelection { get; }

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
        OpenSpaceGameModeInfoAttribute attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();

        DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
        {
            Title = Resources.Utilities_SyncTextureInfo_SelectDirHeader,
            DefaultDirectory = attr.Game?.GetInstallDir(false) ?? FileSystemPath.EmptyPath
        });

        if (result.CanceledByUser)
            return;

        try
        {
            IsLoading = true;

            TextureInfoEditResult syncResult = await Task.Run(() =>
            {
                // Get the settings
                OpenSpaceSettings gameSettings = attr.GetSettings();

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
}
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinarySerializer;
using Microsoft.Win32;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Raving Rabbids configuration
/// </summary>
public class Config_RaymanRavingRabbids_ViewModel : GameOptionsDialog_ConfigPageViewModel
{
    #region Constructor

    public Config_RaymanRavingRabbids_ViewModel(Games game)
    {
        Game = game;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private bool _fullscreenMode;
    private bool _useController;
    private int _screenModeIndex;
    private bool _cheatInvertHor;
    private bool _cheatOldMovie;

    #endregion

    #region Private Constants

    private const string Value_GraphicsMode = "Mode";
    private const string Value_WindowedMode = "WindowedMode";
    private const string Value_DefaultController = "DefaultController";
    private const string Value_ScreenMode = "ScreenMode";
    private const string Value_VideoAdapter = "Video Adapter";
    private const string Value_Format = "Format";

    #endregion

    #region Protected Properties

    protected Games Game { get; }
    protected virtual string Key_BasePath => AppFilePaths.RaymanRavingRabbidsRegistryKey;

    #endregion

    #region Public Properties

    /// <summary>
    /// True if the game should run in fullscreen,
    /// false if it should run in windowed mode
    /// </summary>
    public bool FullscreenMode
    {
        get => _fullscreenMode;
        set
        {
            _fullscreenMode = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// Indicates if a controller should be used by default
    /// </summary>
    public bool UseController
    {
        get => _useController;
        set
        {
            _useController = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The selected screen mode index
    /// </summary>
    public int ScreenModeIndex
    {
        get => _screenModeIndex;
        set
        {
            _screenModeIndex = value;
            UnsavedChanges = true;
        }
    }

    public bool Cheat_InvertHor
    {
        get => _cheatInvertHor;
        set
        {
            _cheatInvertHor = value;
            UnsavedChanges = true;
            HasModifiedCheats = true;
        }
    }

    public bool Cheat_OldMovie
    {
        get => _cheatOldMovie;
        set
        {
            _cheatOldMovie = value;
            UnsavedChanges = true;
            HasModifiedCheats = true;
        }
    }

    public bool CanModifyCheats { get; set; }

    public bool HasModifiedCheats { get; set; }

    #endregion

    #region Protected Methods

    protected virtual string GetGUID()
    {
        return Game switch
        {
            Games.RaymanRavingRabbids => "{05D2C1BC-A857-4493-9BDA-C7707CACB937}",
            Games.Demo_RaymanRavingRabbids => "{F38BE118-6BE7-45b4-9688-58E8712C63FD}",
            _ => throw new Exception("Unsupported game")
        };
    }

    protected void LoadRegistryKey(string keyName, Action<RegistryKey?> loadAction)
    {
        // Open the key. This will return null if it doesn't exist.
        using RegistryKey? key = RegistryHelpers.GetKeyFromFullPath(RegistryHelpers.CombinePaths(Key_BasePath, GetGUID(), keyName), RegistryView.Default);

        // Log
        if (key != null)
            Logger.Info("The key {0} has been opened", keyName);
        else
            Logger.Info("The key {0} for {1} does not exist. Default values will be used.", keyName, Game);

        // Load the values
        loadAction(key);
    }

    protected void SaveRegistryKey(string keyName, Action<RegistryKey> saveAction)
    {
        // Get the key path
        var keyPath = RegistryHelpers.CombinePaths(Key_BasePath, GetGUID(), keyName);

        RegistryKey? key;

        // Create the key if it doesn't exist
        if (!RegistryHelpers.KeyExists(keyPath))
        {
            key = RegistryHelpers.CreateRegistryKey(keyPath, RegistryView.Default, true);

            Logger.Info("The Registry key {0} has been created", key?.Name);
        }
        else
        {
            key = RegistryHelpers.GetKeyFromFullPath(keyPath, RegistryView.Default, true);
        }

        using (key)
        {
            if (key == null)
                throw new Exception("The Registry key could not be created");

            Logger.Info("The key {0} has been opened", key.Name);

            saveAction(key);
        }
    }

    protected int GetValue_DWORD(RegistryKey? key, string name, int defaultValue) => (int)(key?.GetValue(name, defaultValue) ?? defaultValue);

    protected override object GetPageUI() => new Config_RaymanRavingRabbids_Control()
    {
        DataContext = this
    };

    /// <summary>
    /// Loads and sets up the current configuration properties
    /// </summary>
    /// <returns>The task</returns>
    protected override Task LoadAsync()
    {
        Logger.Info("{0} config is being set up", Game);

        GraphicsMode.MinGraphicsWidth = 640;
        GraphicsMode.MinGraphicsHeight = 480;
        GraphicsMode.SortMode = GraphicsModeSelectionViewModel.GraphicsSortMode.TotalPixels;
        GraphicsMode.SortDirection = GraphicsModeSelectionViewModel.GraphicsSortDirection.Ascending;
        GraphicsMode.AllowCustomGraphicsMode = false;
        GraphicsMode.IncludeRefreshRate = true;
        GraphicsMode.FilterMode = GraphicsModeSelectionViewModel.GraphicsFilterMode.WidthOrHeight;
        GraphicsMode.GetAvailableResolutions();

        LoadRegistryKey("Basic video", key =>
        {
            GraphicsMode.SelectedGraphicsModeIndex = GetValue_DWORD(key, Value_GraphicsMode, 0);
            FullscreenMode = GetValue_DWORD(key, Value_WindowedMode, 0) == 0;
            UseController = GetValue_DWORD(key, Value_DefaultController, 0) > 0;
            ScreenModeIndex = GetValue_DWORD(key, Value_ScreenMode, 1) - 1;
        });

        CanModifyCheats = true;

        try
        {
            Logger.Info("Loading cheats from save file");

            ProgressionDirectory saveDir = new(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.sav");
            FileSystemPath saveDirPath = saveDir.GetReadSearchPattern(ProgramDataSource.Auto).DirPath;

            using RCPContext context = new(saveDirPath);

            RRR_SaveFile? saveData = context.ReadFileData<RRR_SaveFile>("Rayman4.sav", new RRR_SaveEncoder());

            if (saveData != null)
            {
                Cheat_InvertHor = saveData.ConfigSlot.Univers.VID_gi_InvertHoriz != 0;
                Cheat_OldMovie = saveData.ConfigSlot.Univers.VID_gi_ModeOldMovie != 0;
            }
            else
            {
                Logger.Info("Save file could not be loaded");
                CanModifyCheats = false;
            }
        }
        catch (Exception ex)
        {
            CanModifyCheats = false;
            Logger.Info(ex, "Error when loading RRR save");
        }

        UnsavedChanges = false;
        HasModifiedCheats = false;

        Logger.Info("All values have been loaded");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the changes
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("{0} configuration is saving...", Game);

        try
        {
            SaveRegistryKey("Basic video", key =>
            {
                key.SetValue(Value_GraphicsMode, GraphicsMode.SelectedGraphicsModeIndex);
                key.SetValue(Value_WindowedMode, FullscreenMode ? 0 : 1);
                key.SetValue(Value_DefaultController, UseController ? 1 : 0);
                key.SetValue(Value_ScreenMode, ScreenModeIndex + 1);
                // NOTE: We're always setting this to 0 for now since the way the game handles it is unreliable. This should work in the
                //       majority of cases. It might be a problem if the video adapter used by RCP differs from the one the game sees as index 0
                key.SetValue(Value_VideoAdapter, 0);
                // NOTE: We default this to D3DFMT_X8R8G8B8 (https://docs.microsoft.com/en-us/windows/win32/direct3d9/d3dformat)
                key.SetValue(Value_Format, 22);
            });

            // Only modify the save files if the cheat options have been modified
            if (HasModifiedCheats && CanModifyCheats)
            {
                Logger.Info("Saving cheats to save files");

                ProgressionDirectory saveDir = new(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.sav");

                // TODO: Have this respect the data source setting in the progression page? Save for reading then.
                foreach (FileSystemPath saveDirPath in saveDir.GetWriteSearchPatterns(ProgramDataSource.Auto).Select(x => x.DirPath))
                {
                    using RCPContext context = new(saveDirPath);

                    const string saveFileName = "Rayman4.sav";

                    PhysicalFile file = new EncodedLinearFile(context, saveFileName, new RRR_SaveEncoder());

                    if (!file.SourceFileExists)
                    {
                        Logger.Warn("Cheats could not be saved to save not being found");
                        continue;
                    }

                    context.AddFile(file);

                    RRR_SaveFile saveData = FileFactory.Read<RRR_SaveFile>(context, saveFileName);

                    foreach (RRR_SaveSlot slot in saveData.StorySlots.Append(saveData.ConfigSlot).Append(saveData.ScoreSlot))
                    {
                        slot.Univers.VID_gi_InvertHoriz = Cheat_InvertHor ? 1 : 0;
                        slot.Univers.VID_gi_ModeOldMovie = Cheat_OldMovie ? 1 : 0;
                    }

                    FileFactory.Write<RRR_SaveFile>(context, saveFileName, saveData);
                }
            }

            Logger.Info("{0} configuration has been saved", Game);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving {0} registry data", Game);
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Config_SaveRRRError, Resources.Config_SaveErrorHeader);
            return false;
        }
    }

    #endregion
}
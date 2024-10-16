﻿using System.IO;
using BinarySerializer;
using Microsoft.Win32;

namespace RayCarrot.RCP.Metro.Games.Settings;

/// <summary>
/// View model for the Rayman Raving Rabbids settings
/// </summary>
public abstract class BaseRaymanRavingRabbidsSettingsViewModel : GameSettingsViewModel
{
    #region Constructor

    protected BaseRaymanRavingRabbidsSettingsViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        GraphicsMode = new GraphicsModeSelectionViewModel();
        GraphicsMode.GraphicsModeChanged += (_, _) => UnsavedChanges = true;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

    protected virtual string Key_BasePath => AppFilePaths.RaymanRavingRabbidsRegistryKey;

    #endregion

    #region Public Properties

    public GraphicsModeSelectionViewModel GraphicsMode { get; }

    public bool FullscreenMode { get; set; }
    public bool UseController { get; set; }
    public int ScreenModeIndex { get; set; }

    public bool Cheat_InvertHor { get; set; }
    public bool Cheat_OldMovie { get; set; }

    public bool CanModifyCheats { get; set; }
    public bool HasModifiedCheats { get; set; }

    #endregion

    #region Protected Methods

    protected abstract string GetGUID();

    protected void LoadRegistryKey(string keyName, Action<RegistryKey?> loadAction)
    {
        // Open the key. This will return null if it doesn't exist.
        using RegistryKey? key = RegistryHelpers.GetKeyFromFullPath(RegistryHelpers.CombinePaths(Key_BasePath, GetGUID(), keyName), RegistryView.Default);

        // Log
        if (key != null)
            Logger.Info("The key {0} has been opened", keyName);
        else
            Logger.Info("The key {0} for {1} does not exist. Default values will be used.", keyName, GameInstallation.FullId);

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

    protected override Task LoadAsync()
    {
        AddSettingsLocation(LinkItemViewModel.LinkType.RegistryKey, RegistryHelpers.CombinePaths(Key_BasePath, GetGUID()));

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

            ProgressionDirectory saveDir = new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, "*.sav");
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

        return Task.CompletedTask;
    }

    protected override Task SaveAsync()
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

            ProgressionDirectory saveDir = new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, "*.sav");

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

        return Task.CompletedTask;
    }

    protected override void SettingsPropertyChanged(string propertyName)
    {
        if (propertyName is
            nameof(FullscreenMode) or
            nameof(UseController) or
            nameof(ScreenModeIndex) or
            nameof(Cheat_InvertHor) or
            nameof(Cheat_OldMovie))
        {
            UnsavedChanges = true;
        }

        if (propertyName is
            nameof(Cheat_InvertHor) or
            nameof(Cheat_OldMovie))
        {
            HasModifiedCheats = true;
        }
    }

    #endregion
}
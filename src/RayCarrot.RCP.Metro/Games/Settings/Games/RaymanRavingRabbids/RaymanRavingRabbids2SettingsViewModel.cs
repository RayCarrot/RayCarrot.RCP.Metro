﻿using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Settings;

public class RaymanRavingRabbids2SettingsViewModel : BaseRaymanRavingRabbidsSettingsViewModel
{
    #region Constructor

    public RaymanRavingRabbids2SettingsViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    #endregion

    #region Private Constants

    private const string Value_GraphicsMode = "Resolution";
    private const string Value_WindowedMode = "WindowedMode";
    private const string Value_VideoAdapter = "Video Adapter";
    private const string Value_Format = "Format";

    #endregion

    #region Protected Properties

    protected override string Key_BasePath => AppFilePaths.RaymanRavingRabbids2RegistryKey;

    #endregion

    #region Protected Methods

    protected override string GetGUID()
    {
        // The game hard-codes this based on what the game mode is specified as in the launch arguments
        return GameInstallation.GetValue(GameDataKey.RRR2_LaunchMode, RaymanRavingRabbids2LaunchMode.AllGames) switch
        {
            RaymanRavingRabbids2LaunchMode.Orange => "{1D769438-429C-4309-931D-A643DF9C57D9}",
            RaymanRavingRabbids2LaunchMode.Red => "{D3AF3B47-FF38-4296-8456-759D6C165934}",
            RaymanRavingRabbids2LaunchMode.Green => "{9F61B3F4-B5B6-4049-84CB-2A7CFA6BBCA4}",
            RaymanRavingRabbids2LaunchMode.Blue => "{B1519A9F-864D-47FF-B69F-65F47CA911B0}",
            RaymanRavingRabbids2LaunchMode.AllGames or _ => "{B864EBC6-9DB8-4A5E-9F08-B0CE286785EC}",
        };
    }

    protected override Task LoadAsync()
    {
        GraphicsMode.MinGraphicsWidth = 800;
        GraphicsMode.MinGraphicsHeight = 600;
        GraphicsMode.MaxGraphicsWidth = 1280;
        GraphicsMode.MaxGraphicsHeight = 1024;
        GraphicsMode.SortMode = GraphicsModeSelectionViewModel.GraphicsSortMode.TotalPixels;
        GraphicsMode.SortDirection = GraphicsModeSelectionViewModel.GraphicsSortDirection.Ascending;
        GraphicsMode.AllowCustomGraphicsMode = false;
        GraphicsMode.IncludeRefreshRate = true;
        GraphicsMode.FilterMode = GraphicsModeSelectionViewModel.GraphicsFilterMode.WidthAndHeight;
        GraphicsMode.GetAvailableResolutions();

        LoadRegistryKey("Video", key =>
        {
            GraphicsMode.SelectedGraphicsModeIndex = GetValue_DWORD(key, Value_GraphicsMode, 0);
            FullscreenMode = GetValue_DWORD(key, Value_WindowedMode, 0) == 0;
            //ScreenModeIndex = GetValue_DWORD(key, Value_ScreenMode, 1) - 1;
        });
        //LoadRegistryKey("Controls", key =>
        //{
        //    UseController = GetValue_DWORD(key, Value_DefaultController, 0) > 0;
        //});

        UnsavedChanges = false;

        return Task.CompletedTask;
    }

    protected override Task SaveAsync()
    {
        SaveRegistryKey("Video", key =>
        {
            key.SetValue(Value_GraphicsMode, GraphicsMode.SelectedGraphicsModeIndex);
            key.SetValue(Value_WindowedMode, FullscreenMode ? 0 : 1);
            //key.SetValue(Value_ScreenMode, ScreenModeIndex + 1);
            // NOTE: We're always setting this to 0 for now since the way the game handles it is unreliable. This should work in the
            //       majority of cases. It might be a problem if the video adapter used by RCP differs from the one the game sees as index 0
            key.SetValue(Value_VideoAdapter, 0);
            // NOTE: We default this to D3DFMT_X8R8G8B8 (https://docs.microsoft.com/en-us/windows/win32/direct3d9/d3dformat)
            key.SetValue(Value_Format, 22);
        });
        //SaveRegistryKey("Controls", key =>
        //{
        //    key.SetValue(Value_DefaultController, UseController ? 1 : 0);
        //});

        return Task.CompletedTask;
    }

    #endregion
}
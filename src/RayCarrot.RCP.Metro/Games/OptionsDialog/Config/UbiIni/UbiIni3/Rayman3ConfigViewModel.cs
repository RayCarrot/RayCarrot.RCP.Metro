﻿#nullable disable
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman 3 configuration
/// </summary>
public class Rayman3ConfigViewModel : UbiIni3ConfigBaseViewModel<Rayman3IniAppData, R3Languages>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation</param>
    public Rayman3ConfigViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    { }

    #endregion

    #region Protected Override Properties

    /// <summary>
    /// The available game patches
    /// </summary>
    protected override FilePatcher_Patch[] Patches => null;

    #endregion

    #region Public Override Properties

    /// <summary>
    /// Indicates if <see cref="UbiIni3ConfigBaseViewModel{Handler,Language}.HorizontalAxis"/> and <see cref="UbiIni3ConfigBaseViewModel{Handler,Language}.VerticalAxis"/> are available
    /// </summary>
    public override bool HasControllerConfig => true;

    /// <summary>
    /// Indicates if <see cref="UbiIni3ConfigBaseViewModel{Handler,Language}.ModemQualityIndex"/> is available
    /// </summary>
    public override bool HasNetworkConfig => false;

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Loads the <see cref="UbiIniBaseConfigViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The config data</returns>
    protected override Rayman3IniAppData CreateConfig()
    {
        // Load the configuration data
        return new Rayman3IniAppData(AppFilePaths.UbiIniPath);
    }

    /// <summary>
    /// Imports the <see cref="UbiIniBaseConfigViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected override Task ImportConfigAsync()
    {
        if (CpaDisplayMode.TryParse(ConfigData.GLI_Mode, out CpaDisplayMode displayMode))
        {
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(displayMode.Width, displayMode.Height);
            FullscreenMode = displayMode.IsFullscreen;
            IsTextures32Bit = displayMode.BitsPerPixel != 16;
        }
        else
        {
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(800, 600);
            FullscreenMode = true;
            IsTextures32Bit = true;
        }

        TriLinear = ConfigData.TriLinear != 0;
        TnL = ConfigData.TnL != 0;
        CompressedTextures = ConfigData.TexturesCompressed != 0;
        VideoQuality = ConfigData.Video_WantedQuality;
        AutoVideoQuality = ConfigData.Video_AutoAdjustQuality != 0;
        IsVideo32Bpp = ConfigData.Video_BPP != 16;
        CurrentLanguage = Enum.TryParse(ConfigData.Language, out R3Languages lang) ? lang : R3Languages.English;
        VerticalAxis = ConfigData.Camera_VerticalAxis;
        HorizontalAxis = ConfigData.Camera_HorizontalAxis;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the <see cref="UbiIniBaseConfigViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected override Task UpdateConfigAsync()
    {
        ConfigData.GLI_Mode = new CpaDisplayMode()
        {
            BitsPerPixel = IsTextures32Bit ? 32 : 16,
            IsFullscreen = FullscreenMode,
            Width = GraphicsMode.Width,
            Height = GraphicsMode.Height,
        }.ToString();

        ConfigData.TriLinear = TriLinear ? 1 : 0;
        ConfigData.TnL = TnL ? 1 : 0;
        ConfigData.TexturesCompressed = CompressedTextures ? 1 : 0;
        ConfigData.Video_WantedQuality = VideoQuality;
        ConfigData.Video_AutoAdjustQuality = AutoVideoQuality ? 1 : 0;
        ConfigData.Video_BPP = IsVideo32Bpp ? 32 : 16;
        ConfigData.Language = CurrentLanguage.ToString();
        ConfigData.Camera_VerticalAxis = VerticalAxis;
        ConfigData.Camera_HorizontalAxis = HorizontalAxis;
        ConfigData.TexturesFile = $"Tex{(IsTextures32Bit ? 32 : 16)}.cnt";

        return Task.CompletedTask;
    }

    #endregion
}
﻿#nullable disable
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman 3 configuration
/// </summary>
public class Rayman3ConfigViewModel : UbiIni3ConfigBaseViewModel<UbiIniData_Rayman3, R3Languages>
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
    protected override Task<UbiIniData_Rayman3> LoadConfigAsync()
    {
        // Load the configuration data
        return Task.FromResult(new UbiIniData_Rayman3(AppFilePaths.UbiIniPath1));
    }

    /// <summary>
    /// Imports the <see cref="UbiIniBaseConfigViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected override Task ImportConfigAsync()
    {
        var gliMode = ConfigData.FormattedGLI_Mode;

        if (gliMode != null)
        {
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(gliMode.ResX, gliMode.ResY);
            FullscreenMode = !gliMode.IsWindowed;
            IsTextures32Bit = gliMode.ColorMode != 16;
        }
        else
        {
            GraphicsMode.SelectedGraphicsMode = new GraphicsMode(800, 600);
            FullscreenMode = true;
            IsTextures32Bit = true;
        }

        TriLinear = ConfigData.FormattedTriLinear;
        TnL = ConfigData.FormattedTnL;
        CompressedTextures = ConfigData.FormattedTexturesCompressed;
        VideoQuality = ConfigData.FormattedVideo_WantedQuality ?? 4;
        AutoVideoQuality = ConfigData.FormattedVideo_AutoAdjustQuality;
        IsVideo32Bpp = ConfigData.FormattedVideo_BPP != 16;
        CurrentLanguage = ConfigData.FormattedLanguage ?? R3Languages.English;
        VerticalAxis = ConfigData.FormattedCamera_VerticalAxis ?? 5;
        HorizontalAxis = ConfigData.FormattedCamera_HorizontalAxis ?? 2;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the <see cref="UbiIniBaseConfigViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected override Task UpdateConfigAsync()
    {
        ConfigData.GLI_Mode = new RayGLI_Mode()
        {
            ColorMode = IsTextures32Bit ? 32 : 16,
            IsWindowed = !FullscreenMode,
            ResX = GraphicsMode.Width,
            ResY = GraphicsMode.Height,
        }.ToString();

        ConfigData.FormattedTriLinear = TriLinear;
        ConfigData.FormattedTnL = TnL;
        ConfigData.FormattedTexturesCompressed = CompressedTextures;
        ConfigData.Video_WantedQuality = VideoQuality.ToString();
        ConfigData.FormattedVideo_AutoAdjustQuality = AutoVideoQuality;
        ConfigData.Video_BPP = IsVideo32Bpp ? "32" : "16";
        ConfigData.Language = CurrentLanguage.ToString();
        ConfigData.Camera_VerticalAxis = VerticalAxis.ToString();
        ConfigData.Camera_HorizontalAxis = HorizontalAxis.ToString();
        ConfigData.TexturesFile = $"Tex{(IsTextures32Bit ? 32 : 16)}.cnt";

        return Task.CompletedTask;
    }

    #endregion
}
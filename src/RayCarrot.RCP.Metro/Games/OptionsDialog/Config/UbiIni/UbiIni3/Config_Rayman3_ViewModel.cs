#nullable disable
using System.Threading.Tasks;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman 3 configuration
/// </summary>
public class Config_Rayman3_ViewModel : Config_UbiIni3_BaseViewModel<R3UbiIniHandler, R3Languages>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="game">The game</param>
    public Config_Rayman3_ViewModel(Games game = Games.Rayman3) : base(game)
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
    /// Indicates if <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.DynamicShadows"/> and <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.StaticShadows"/> are available
    /// </summary>
    public override bool HasShadowConfig => true;

    /// <summary>
    /// Indicates if <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.HorizontalAxis"/> and <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.VerticalAxis"/> are available
    /// </summary>
    public override bool HasControllerConfig => true;

    /// <summary>
    /// Indicates if <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.ModemQualityIndex"/> is available
    /// </summary>
    public override bool HasNetworkConfig => false;

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Loads the <see cref="Config_UbiIni_BaseViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The config data</returns>
    protected override Task<R3UbiIniHandler> LoadConfigAsync()
    {
        // Load the configuration data
        return Task.FromResult(new R3UbiIniHandler(AppFilePaths.UbiIniPath1));
    }

    /// <summary>
    /// Imports the <see cref="Config_UbiIni_BaseViewModel{Handler}.ConfigData"/>
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
        DynamicShadows = ConfigData.FormattedDynamicShadows;
        StaticShadows = ConfigData.FormattedStaticShadows;
        VerticalAxis = ConfigData.FormattedCamera_VerticalAxis ?? 5;
        HorizontalAxis = ConfigData.FormattedCamera_HorizontalAxis ?? 2;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the <see cref="Config_UbiIni_BaseViewModel{Handler}.ConfigData"/>
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
        ConfigData.FormattedDynamicShadows = DynamicShadows;
        ConfigData.FormattedStaticShadows = StaticShadows;
        ConfigData.Camera_VerticalAxis = VerticalAxis.ToString();
        ConfigData.Camera_HorizontalAxis = HorizontalAxis.ToString();
        ConfigData.TexturesFile = $"Tex{(IsTextures32Bit ? 32 : 16)}.cnt";

        return Task.CompletedTask;
    }

    #endregion
}
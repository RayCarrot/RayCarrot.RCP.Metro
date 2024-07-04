#nullable disable
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman M configuration
/// </summary>
public class RaymanMConfigViewModel : UbiIni3ConfigBaseViewModel<RaymanMIniAppData, RMLanguages>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation</param>
    public RaymanMConfigViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {

    }

    #endregion

    #region Protected Override Properties

    /// <summary>
    /// The available game patches
    /// </summary>
    protected override FilePatcher_Patch[] Patches => new FilePatcher_Patch[]
    {
        new FilePatcher_Patch(0x1C4000, new FilePatcher_Patch.PatchEntry[]
        {
            new FilePatcher_Patch.PatchEntry(0x263D, new byte[]
            {
                0xE8,
                0x3E,
                0xBE,
                0x02,
                0x00,
            }, new byte[]
            {
                0xE9,
                0x00,
                0x00,
                0x00,
                0x00,
            })
        }), 
        new FilePatcher_Patch(0x1C3000, new FilePatcher_Patch.PatchEntry[]
        {
            new FilePatcher_Patch.PatchEntry(0x21EE, new byte[]
            {
                0xE8,
                0xEd,
                0xBD,
                0x02,
                0x00,
            }, new byte[]
            {
                0xE9,
                0x00,
                0x00,
                0x00,
                0x00,
            })
        }), 
    };

    #endregion

    #region Public Override Properties

    /// <summary>
    /// Indicates if <see cref="UbiIni3ConfigBaseViewModel{Handler,Language}.HorizontalAxis"/> and <see cref="UbiIni3ConfigBaseViewModel{Handler,Language}.VerticalAxis"/> are available
    /// </summary>
    public override bool HasControllerConfig => false;

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
    protected override RaymanMIniAppData CreateConfig()
    {
        // Load the configuration data
        return new RaymanMIniAppData(AppFilePaths.UbiIniPath1, isDemo: false);
    }

    /// <summary>
    /// Imports the <see cref="UbiIniBaseConfigViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected override Task ImportConfigAsync()
    {
        RayGLI_Mode gliMode = RayGLI_Mode.Parse(ConfigData.GLI_Mode);

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

        TriLinear = ConfigData.TriLinear != 0;
        TnL = ConfigData.TnL != 0;
        CompressedTextures = ConfigData.TexturesCompressed != 0;
        VideoQuality = ConfigData.Video_WantedQuality;
        AutoVideoQuality = ConfigData.Video_AutoAdjustQuality != 0;
        IsVideo32Bpp = ConfigData.Video_BPP != 16;
        CurrentLanguage = Enum.TryParse(ConfigData.Language, out RMLanguages lang) ? lang : RMLanguages.English;

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

        ConfigData.TriLinear = TriLinear ? 1 : 0;
        ConfigData.TnL = TnL ? 1 : 0;
        ConfigData.TexturesCompressed = CompressedTextures ? 1 : 0;
        ConfigData.Video_WantedQuality = VideoQuality;
        ConfigData.Video_AutoAdjustQuality = AutoVideoQuality ? 1 : 0;
        ConfigData.Video_BPP = IsVideo32Bpp ? 32 : 16;
        ConfigData.Language = CurrentLanguage.ToString();
        ConfigData.TexturesFile = $"Tex{(IsTextures32Bit ? 32 : 16)}.cnt";

        return Task.CompletedTask;
    }

    #endregion
}
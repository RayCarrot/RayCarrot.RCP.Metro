#nullable disable
using System.Threading.Tasks;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Arena configuration
/// </summary>
public class Config_RaymanArena_ViewModel : Config_UbiIni3_BaseViewModel<RAUbiIniHandler, RALanguages>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Config_RaymanArena_ViewModel() : base(Games.RaymanArena)
    { }

    #endregion

    #region Protected Override Properties

    /// <summary>
    /// The available game patches
    /// </summary>
    protected override FilePatcher_Patch[] Patches => new FilePatcher_Patch[]
    {
        new FilePatcher_Patch(0x22D000, new FilePatcher_Patch.PatchEntry[]
        {
            new FilePatcher_Patch.PatchEntry(
                PatchOffset: 0x2FAE6, 
                OriginalBytes: new byte[]
                {
                    0x05, 0x75, 0x3F, 0x6A, 0x00, 0x6A, 0x00, 0x6A, 
                    0x00, 0x6A, 0x00, 0x6A, 0x00, 0x8D, 0x94, 0x24, 
                    0x30, 0x02, 0x00, 0x00, 0x68, 0x80, 0x00, 0x00, 
                    0x00, 0x8D, 0x44, 0x24, 0x28, 0x52, 0x50, 0xFF, 
                    0x15, 0xD0, 0xD0, 0x5A, 0x00, 0x8D, 0x8C, 0x24, 
                    0x1C, 0x02, 0x00, 0x00, 0x68, 0x94, 0xBE, 0x5D,
                    0x00, 0x51, 0xE8, 0x3B, 0x7A, 0x09, 0x00, 0x83, 
                    0xC4, 0x08, 0x85, 0xC0, 0x0F, 0x84, 0x49, 0x01,
                    0x00, 0x00, 0x43, 0x83, 0xFB, 0x20, 0x7C, 0x96,
                }, 
                PatchRevisions: new FilePatcher_Patch.PatchedBytesRevision[]
                {
                    new FilePatcher_Patch.PatchedBytesRevision(0, new byte[]
                    {
                        0x05, 0x75, 0x3F, 0x6A, 0x00, 0x6A, 0x00, 0x6A,
                        0x00, 0x6A, 0x00, 0x6A, 0x00, 0x8D, 0x94, 0x24,
                        0x30, 0x02, 0x00, 0x00, 0x68, 0x80, 0x00, 0x00,
                        0x00, 0x8D, 0x44, 0x24, 0x28, 0x52, 0x50, 0xFF,
                        0x15, 0xD0, 0xD0, 0x5A, 0x00, 0x8D, 0x8C, 0x24,
                        0x1C, 0x02, 0x00, 0x00, 0x68, 0x94, 0xBE, 0x5D,
                        0x00, 0x51, 0xE8, 0x3B, 0x7A, 0x09, 0x00, 0x83,
                        0xC4, 0x08, 0x85, 0xC0, 0xE9, 0x4A, 0x01, 0x00, // Return even if label doesn't match
                        0x00, 0x00, 0x43, 0x83, 0xFB, 0x20, 0x7C, 0x96,
                    }),
                    new FilePatcher_Patch.PatchedBytesRevision(1, new byte[]
                    {
                        0x03, 0x75, 0x3F, 0x6A, 0x00, 0x6A, 0x00, 0x6A, // Check for DRIVE_FIXED instead of DRIVE_CDROM to work without disc drives
                        0x00, 0x6A, 0x00, 0x6A, 0x00, 0x8D, 0x94, 0x24,
                        0x30, 0x02, 0x00, 0x00, 0x68, 0x80, 0x00, 0x00,
                        0x00, 0x8D, 0x44, 0x24, 0x28, 0x52, 0x50, 0xFF,
                        0x15, 0xD0, 0xD0, 0x5A, 0x00, 0x8D, 0x8C, 0x24,
                        0x1C, 0x02, 0x00, 0x00, 0x68, 0x94, 0xBE, 0x5D,
                        0x00, 0x51, 0xE8, 0x3B, 0x7A, 0x09, 0x00, 0x83,
                        0xC4, 0x08, 0x85, 0xC0, 0xE9, 0x4A, 0x01, 0x00, // Return even if label doesn't match
                        0x00, 0x00, 0x43, 0x83, 0xFB, 0x20, 0x7C, 0x96,
                    }),
                })
        }),
    };

    #endregion

    #region Public Override Properties

    /// <summary>
    /// Indicates if <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.DynamicShadows"/> and <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.StaticShadows"/> are available
    /// </summary>
    public override bool HasShadowConfig => false;

    /// <summary>
    /// Indicates if <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.HorizontalAxis"/> and <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.VerticalAxis"/> are available
    /// </summary>
    public override bool HasControllerConfig => false;

    /// <summary>
    /// Indicates if <see cref="Config_UbiIni3_BaseViewModel{Handler,Language}.ModemQualityIndex"/> is available
    /// </summary>
    public override bool HasNetworkConfig => true;

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Loads the <see cref="Config_UbiIni_BaseViewModel{Handler}.ConfigData"/>
    /// </summary>
    /// <returns>The config data</returns>
    protected override Task<RAUbiIniHandler> LoadConfigAsync()
    {
        // Load the configuration data
        return Task.FromResult(new RAUbiIniHandler(AppFilePaths.UbiIniPath1));
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
        CurrentLanguage = ConfigData.FormattedRALanguage ?? RALanguages.English;
        ModemQualityIndex = ConfigData.FormattedModemQuality ?? 0;

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
        ConfigData.ModemQuality = ModemQualityIndex.ToString();
        ConfigData.TexturesFile = $"Tex{(IsTextures32Bit ? 32 : 16)}.cnt";

        return Task.CompletedTask;
    }

    #endregion
}
using System.Threading.Tasks;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Arena configuration
    /// </summary>
    public class RaymanArenaConfigViewModel : Ray_M_Arena_3_UbiIniBaseConfigViewModel<RAUbiIniHandler, RALanguages>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanArenaConfigViewModel() : base(Games.RaymanArena)
        { }

        #endregion

        #region Protected Override Properties

        /// <summary>
        /// The offset for when patching the game check or -1 if not available
        /// </summary>
        protected override int PatchGameCheckOffset => 0x2FB10;

        /// <summary>
        /// The original bytes for when patching the game check
        /// </summary>
        protected override byte[] PatchGameCheckOriginalBytes => new byte[]
        {
            0x00,
            0x00,
            0x68,
            0x94,
            0xbe,
            0x5d,
            0x00,
            0x51,
            0xe8,
            0x3b,
            0x7a,
            0x09,
            0x00,
            0x83,
            0xc4,
            0x08,
            0x85,
            0xc0,
            0x0f,
            0x84,
            0x49,
            0x01,
            0x00,
            0x00,
            0x43,
            0x83,
            0xfb,
            0x20,
            0x7c,
            0x96,
            0x8d,
            0x94
        };

        /// <summary>
        /// The patched bytes when patching the game check
        /// </summary>
        protected override byte[] PatchGameCheckPatchedBytes => new byte[]
        {
            0x00,
            0x00,
            0x68,
            0x94,
            0xBE,
            0x5D,
            0x00,
            0x51,
            0xE8,
            0x3B,
            0x7A,
            0x09,
            0x00,
            0x83,
            0xC4,
            0x08,
            0x85,
            0xC0,
            0xE9,
            0x4A,
            0x01,
            0x00,
            0x00,
            0x00,
            0x43,
            0x83,
            0xFB,
            0x20,
            0x7C,
            0x96,
            0x8D,
            0x94
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// Indicates if <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.DynamicShadows"/> and <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.StaticShadows"/> are available
        /// </summary>
        public override bool HasShadowConfig => false;

        /// <summary>
        /// Indicates if <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.HorizontalAxis"/> and <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.VerticalAxis"/> are available
        /// </summary>
        public override bool HasControllerConfig => false;

        /// <summary>
        /// Indicates if <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.ModemQualityIndex"/> is available
        /// </summary>
        public override bool HasNetworkConfig => true;

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the <see cref="BaseUbiIniGameConfigViewModel{Handler}.ConfigData"/>
        /// </summary>
        /// <returns>The config data</returns>
        protected override Task<RAUbiIniHandler> LoadConfigAsync()
        {
            // Load the configuration data
            return Task.FromResult(new RAUbiIniHandler(CommonPaths.UbiIniPath1));
        }

        /// <summary>
        /// Imports the <see cref="BaseUbiIniGameConfigViewModel{Handler}.ConfigData"/>
        /// </summary>
        /// <returns>The task</returns>
        protected override Task ImportConfigAsync()
        {
            var gliMode = ConfigData.FormattedGLI_Mode;

            if (gliMode != null)
            {
                ResX = gliMode.ResX;
                ResY = gliMode.ResY;
                FullscreenMode = !gliMode.IsWindowed;
                IsTextures32Bit = gliMode.ColorMode != 16;
            }
            else
            {
                LockToScreenRes = true;
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
        /// Updates the <see cref="BaseUbiIniGameConfigViewModel{Handler}.ConfigData"/>
        /// </summary>
        /// <returns>The task</returns>
        protected override Task UpdateConfigAsync()
        {
            ConfigData.GLI_Mode = new RayGLI_Mode()
            {
                ColorMode = IsTextures32Bit ? 32 : 16,
                IsWindowed = !FullscreenMode,
                ResX = ResX,
                ResY = ResY
            }.ToString();

            ConfigData.FormattedTriLinear = TriLinear;
            ConfigData.FormattedTnL = TnL;
            ConfigData.FormattedTexturesCompressed = CompressedTextures;
            ConfigData.Video_WantedQuality = VideoQuality.ToString();
            ConfigData.FormattedVideo_AutoAdjustQuality = AutoVideoQuality;
            ConfigData.Video_BPP = IsVideo32Bpp ? "32" : "16";
            ConfigData.Language = CurrentLanguage.ToString();
            ConfigData.ModemQuality = ModemQualityIndex.ToString();

            return Task.CompletedTask;
        }

        #endregion
    }
}
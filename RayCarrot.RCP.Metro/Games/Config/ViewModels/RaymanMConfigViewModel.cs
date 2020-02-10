using System.Threading.Tasks;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman M configuration
    /// </summary>
    public class RaymanMConfigViewModel : Ray_M_Arena_3_UbiIniBaseConfigViewModel<RMUbiIniHandler, RMLanguages>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        public RaymanMConfigViewModel(Games game = Games.RaymanM) : base(game)
        {

        }

        #endregion

        #region Protected Override Properties

        /// <summary>
        /// The offset for when patching the game check or -1 if not available
        /// </summary>
        protected override int PatchGameCheckOffset => 9776;

        /// <summary>
        /// The original bytes for when patching the game check
        /// </summary>
        protected override byte[] PatchGameCheckOriginalBytes => new byte[]
        {
            0x00,
            0x00,
            0x83,
            0xC4,
            0x0C,
            0x6A,
            0xFF,
            0xFF,
            0x15,
            0x68,
            0x32,
            0x58,
            0x00,
            0xE8,
            0x3E,
            0xBE,
            0x02,
            0x00,
            0xE8,
            0x2C,
            0xF1,
            0xFF,
            0xFF,
            0x85,
            0xC0,
            0x75,
            0x0F,
            0x5F,
            0x5E,
            0x83,
            0xC8,
            0xFF
        };

        /// <summary>
        /// The patched bytes when patching the game check
        /// </summary>
        protected override byte[] PatchGameCheckPatchedBytes => new byte[]
        {
            0x00,
            0x00,
            0x83,
            0xC4,
            0x0C,
            0x6A,
            0xFF,
            0xFF,
            0x15,
            0x68,
            0x32,
            0x58,
            0x00,
            0xE9,
            0x00,
            0x00,
            0x00,
            0x00,
            0xE8,
            0x2C,
            0xF1,
            0xFF,
            0xFF,
            0x85,
            0xC0,
            0x75,
            0x0F,
            0x5F,
            0x5E,
            0x83,
            0xC8,
            0xFF
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
        public override bool HasNetworkConfig => false;

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the <see cref="BaseUbiIniGameConfigViewModel{Handler}.ConfigData"/>
        /// </summary>
        /// <returns>The config data</returns>
        protected override Task<RMUbiIniHandler> LoadConfigAsync()
        {
            // Load the configuration data
            return Task.FromResult(new RMUbiIniHandler(CommonPaths.UbiIniPath1));
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
            CurrentLanguage = ConfigData.FormattedRMLanguage ?? RMLanguages.English;

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
            ConfigData.TexturesFile = $"Tex{(IsTextures32Bit ? 32 : 16)}.cnt";

            return Task.CompletedTask;
        }

        #endregion
    }
}
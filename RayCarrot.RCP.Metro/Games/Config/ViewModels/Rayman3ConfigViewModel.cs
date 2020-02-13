using RayCarrot.Rayman;
using System.Threading.Tasks;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 3 configuration
    /// </summary>
    public class Rayman3ConfigViewModel : Ray_M_Arena_3_UbiIniBaseConfigViewModel<R3UbiIniHandler, R3Languages>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        public Rayman3ConfigViewModel(Games game = Games.Rayman3) : base(game)
        { }

        #endregion

        #region Protected Override Properties

        /// <summary>
        /// The offset for when patching the game check or -1 if not available
        /// </summary>
        protected override int PatchGameCheckOffset => -1;

        /// <summary>
        /// The original bytes for when patching the game check
        /// </summary>
        protected override byte[] PatchGameCheckOriginalBytes => null;

        /// <summary>
        /// The patched bytes when patching the game check
        /// </summary>
        protected override byte[] PatchGameCheckPatchedBytes => null;

        #endregion

        #region Public Override Properties

        /// <summary>
        /// Indicates if <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.DynamicShadows"/> and <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.StaticShadows"/> are available
        /// </summary>
        public override bool HasShadowConfig => true;

        /// <summary>
        /// Indicates if <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.HorizontalAxis"/> and <see cref="Ray_M_Arena_3_UbiIniBaseConfigViewModel{Handler,Language}.VerticalAxis"/> are available
        /// </summary>
        public override bool HasControllerConfig => true;

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
        protected override Task<R3UbiIniHandler> LoadConfigAsync()
        {
            // Load the configuration data
            return Task.FromResult(new R3UbiIniHandler(CommonPaths.UbiIniPath1));
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
            CurrentLanguage = ConfigData.FormattedLanguage ?? R3Languages.English;
            DynamicShadows = ConfigData.FormattedDynamicShadows;
            StaticShadows = ConfigData.FormattedStaticShadows;
            VerticalAxis = ConfigData.FormattedCamera_VerticalAxis ?? 5;
            HorizontalAxis = ConfigData.FormattedCamera_HorizontalAxis ?? 2;

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
            ConfigData.FormattedDynamicShadows = DynamicShadows;
            ConfigData.FormattedStaticShadows = StaticShadows;
            ConfigData.Camera_VerticalAxis = VerticalAxis.ToString();
            ConfigData.Camera_HorizontalAxis = HorizontalAxis.ToString();
            ConfigData.TexturesFile = $"Tex{(IsTextures32Bit ? 32 : 16)}.cnt";

            return Task.CompletedTask;
        }

        #endregion
    }
}
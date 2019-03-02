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
        {
            ModemQualityOptions = new string[]
            {
                "Unknown",
                "Modem 56k",
                "RNIS",
                "xDSL or cable",
                "Local Area Network"
            };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The available modem quality options
        /// </summary>
        public string[] ModemQualityOptions { get; }

        /// <summary>
        /// The current modem quality index
        /// </summary>
        public int ModemQualityIndex { get; set; }

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
using System.Threading.Tasks;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman M demo configuration
    /// </summary>
    public class Config_RaymanMDemo_ViewModel : Config_RaymanM_ViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        public Config_RaymanMDemo_ViewModel() : base(Games.Demo_RaymanM)
        {

        }

        #endregion

        #region Protected Override Properties

        /// <summary>
        /// The available game patches
        /// </summary>
        protected override FilePatcher_Patch[] Patches => null;

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the <see cref="Config_UbiIni_BaseViewModel{Handler}.ConfigData"/>
        /// </summary>
        /// <returns>The config data</returns>
        protected override Task<RMUbiIniHandler> LoadConfigAsync()
        {
            // Load the configuration data
            return Task.FromResult<RMUbiIniHandler>(new RMDemoUbiIniHandler(CommonPaths.UbiIniPath1));
        }

        #endregion
    }
}
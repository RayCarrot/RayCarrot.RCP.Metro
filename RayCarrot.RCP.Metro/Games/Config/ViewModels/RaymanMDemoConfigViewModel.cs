using System.Threading.Tasks;
using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman M demo configuration
    /// </summary>
    public class RaymanMDemoConfigViewModel : RaymanMConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        public RaymanMDemoConfigViewModel() : base(Games.Demo_RaymanM)
        {

        }

        #endregion

        #region Protected Override Properties

        /// <summary>
        /// The offset for when patching the game check or -1 if not available
        /// </summary>
        protected override int PatchGameCheckOffset => -1;

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the <see cref="BaseUbiIniGameConfigViewModel{Handler}.ConfigData"/>
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
using RayCarrot.Extensions;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The API controller manager for the Rayman Control Panel Metro
    /// </summary>
    public class RCPMetroAPIControllerManager : BaseAPIControllerManager
    {
        /// <summary>
        /// Gets the available game data for the specified game, or null if not found
        /// </summary>
        /// <param name="game">The game to get the game data for</param>
        /// <returns>The game data for the specified game, or null if not found</returns>
        public override GameData GetGameData(Games game)
        {
            return RCFRCP.Data.Games.TryGetValue(game);
        }

        /// <summary>
        /// Get the icon path as used in WPF operations
        /// </summary>
        /// <returns>The icon path</returns>
        public override string GetWPFIconPath => RCFRCP.App.WPFApplicationBasePath + "Img/RCP_Metro.ico";

        /// <summary>
        /// The not localized app name
        /// </summary>
        public override string AppDisplayName => "Rayman Control Panel";

        /// <summary>
        /// The app code name
        /// </summary>
        public override string AppCodeName => "RCP_Metro";
    }
}
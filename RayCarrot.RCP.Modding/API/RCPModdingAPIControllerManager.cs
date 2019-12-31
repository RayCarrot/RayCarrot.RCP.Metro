using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// The API controller manager for the Rayman Modding Panel
    /// </summary>
    public class RCPModdingAPIControllerManager : IAPIControllerManager
    {
        /// <summary>
        /// Gets the available game data for the specified game, or null if not found
        /// </summary>
        /// <param name="game">The game to get the game data for</param>
        /// <returns>The game data for the specified game, or null if not found</returns>
        public GameData GetGameData(Games game)
        {
            // TODO: Return data if found from Rayman Control Panel Metro data
            return null;
        }

        /// <summary>
        /// Get the icon path as used in WPF operations
        /// </summary>
        /// <returns>The icon path</returns>
        public string GetWPFIconPath()
        {
            return RCFRCPM.App.WPFApplicationBasePath + "Img/RCP_Modding.ico";
        }
    }
}
using RayCarrot.Extensions;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The API controller manager for the Rayman Control Panel Metro
    /// </summary>
    public class RCPMetroAPIControllerManager : IAPIControllerManager
    {
        /// <summary>
        /// Gets the available game data for the specified game, or null if not found
        /// </summary>
        /// <param name="game">The game to get the game data for</param>
        /// <returns>The game data for the specified game, or null if not found</returns>
        public GameData GetGameData(Games game)
        {
            return RCFRCP.Data.Games.TryGetValue(game);
        }

        /// <summary>
        /// Gets a value indicating if the application is running as administrator
        /// </summary>
        /// <returns>True if the application is running as administrator</returns>
        public bool IsRunningAsAdmin()
        {
            return RCFRCP.App.IsRunningAsAdmin;
        }
    }
}
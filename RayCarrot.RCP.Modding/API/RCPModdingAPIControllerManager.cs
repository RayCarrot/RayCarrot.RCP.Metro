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
        /// Gets a value indicating if the application is running as administrator
        /// </summary>
        /// <returns>True if the application is running as administrator</returns>
        public bool IsRunningAsAdmin()
        {
            return RCFRCPM.App.IsRunningAsAdmin;
        }
    }
}
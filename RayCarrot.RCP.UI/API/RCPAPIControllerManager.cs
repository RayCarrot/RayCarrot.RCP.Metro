using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.UI
{
    /// <summary>
    /// The base API controller manager for the Rayman Control Panel
    /// </summary>
    public abstract class RCPAPIControllerManager : IAPIControllerManager
    {
        /// <summary>
        /// Gets the available game data for the specified game, or null if not found
        /// </summary>
        /// <param name="game">The game to get the game data for</param>
        /// <returns>The game data for the specified game, or null if not found</returns>
        public abstract GameData GetGameData(Games game);

        /// <summary>
        /// Gets a value indicating if the application is running as administrator
        /// </summary>
        /// <returns>True if the application is running as administrator</returns>
        public bool IsRunningAsAdmin()
        {
            return RCFRCPUI.App.IsRunningAsAdmin;
        }

    }
}
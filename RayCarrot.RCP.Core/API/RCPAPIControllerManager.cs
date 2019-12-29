namespace RayCarrot.RCP.Core
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
    }
}
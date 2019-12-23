namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Defines the API controller manager to use for the Rayman Control Panel
    /// </summary>
    public interface IAPIControllerManager
    {
        /// <summary>
        /// Gets the available game data for the specified game, or null if not found
        /// </summary>
        /// <param name="game">The game to get the game data for</param>
        /// <returns>The game data for the specified game, or null if not found</returns>
        GameData GetGameData(Games game);

        /// <summary>
        /// Gets a value indicating if the application is running as administrator
        /// </summary>
        /// <returns>True if the application is running as administrator</returns>
        bool IsRunningAsAdmin();
    }
}
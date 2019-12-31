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
        /// Get the icon path as used in WPF operations
        /// </summary>
        /// <returns>The icon path</returns>
        string GetWPFIconPath();
    }
}
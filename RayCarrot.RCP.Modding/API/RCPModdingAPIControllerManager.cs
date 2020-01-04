using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// The API controller manager for the Rayman Modding Panel
    /// </summary>
    public class RCPModdingAPIControllerManager : BaseAPIControllerManager
    {
        /// <summary>
        /// Gets the available game data for the specified game, or null if not found
        /// </summary>
        /// <param name="game">The game to get the game data for</param>
        /// <returns>The game data for the specified game, or null if not found</returns>
        public override GameData GetGameData(Games game)
        {
            // TODO: Return data if found from Rayman Control Panel Metro data
            return null;
        }

        /// <summary>
        /// Get the icon path as used in WPF operations
        /// </summary>
        /// <returns>The icon path</returns>
        public override string GetWPFIconPath => RCFRCPM.App.WPFApplicationBasePath + "Img/RCP_Modding.ico";

        /// <summary>
        /// The not localized app name
        /// </summary>
        public override string AppDisplayName => "Rayman Modding Panel";

        /// <summary>
        /// The app code name
        /// </summary>
        public override string AppCodeName => "RCP_Modding";
    }
}
using RayCarrot.Windows.Registry;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Base class for an API controller manager to use for the Rayman Control Panel
    /// </summary>
    public abstract class BaseAPIControllerManager
    {
        /// <summary>
        /// Gets the available game data for the specified game, or null if not found
        /// </summary>
        /// <param name="game">The game to get the game data for</param>
        /// <returns>The game data for the specified game, or null if not found</returns>
        public abstract GameData GetGameData(Games game);

        /// <summary>
        /// Get the icon path as used in WPF operations
        /// </summary>
        public abstract string GetWPFIconPath { get; }

        /// <summary>
        /// The not localized app name to display
        /// </summary>
        public abstract string AppDisplayName { get; }

        /// <summary>
        /// The app code name
        /// </summary>
        public abstract string AppCodeName { get; }

        /// <summary>
        /// The Registry base key
        /// </summary>
        public string RegistryBaseKey => RCFRegistryPaths.BasePath + $"\\{AppCodeName}";

        /// <summary>
        /// The license accepted value name
        /// </summary>
        public string RegistryLicenseValue => "LicenseAccepted";
    }
}
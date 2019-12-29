using System.Windows;
using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Core
{
    // IDEA: Remove settings and instead have each project override the class with these abstract fields

    /// <summary>
    /// The UI settings for the API controller
    /// </summary>
    public abstract class APIControllerUISettings
    {
        /// <summary>
        /// The application base path
        /// </summary>
        public abstract string ApplicationBasePath { get; }

        /// <summary>
        /// The path for the icon to use on Windows
        /// </summary>
        public virtual string WindowIconPath => "pack://application:,,,/RayCarrot.RCP.Core;component/Images/Rayman Control Panel Icon.ico";

        /// <summary>
        /// Indicates if Window transitions are enabled
        /// </summary>
        public virtual bool AreWindowTransitionsEnabled => true;

        /// <summary>
        /// The minimum exception level to display
        /// </summary>
        public abstract ExceptionLevel DisplayExceptionLevel { get; }

        /// <summary>
        /// Optional method to run when setting up a Window
        /// </summary>
        /// <param name="window">The Window which is being set up</param>
        public virtual void OnWindowSetup(Window window) { }

        /// <summary>
        /// Gets the current UI settings instance
        /// </summary>
        /// <returns>The UI settings instance</returns>
        public static APIControllerUISettings GetSettings()
        {
            return APIController.Controller.Settings.GetUISettings();
        }
    }
}
using System.Windows;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// The API controller UI settings for the Rayman Modding Panel
    /// </summary>
    public class RCPModdingAPIControllerUISettings : APIControllerUISettings
    {
        /// <summary>
        /// The application base path
        /// </summary>
        public override string ApplicationBasePath => "pack://application:,,,/RayCarrot.RCP.Modding;component/";

        /// <summary>
        /// Indicates if Window transitions are enabled
        /// </summary>
        public override bool AreWindowTransitionsEnabled => RCFRCPM.Data?.EnableAnimations ?? true;

        /// <summary>
        /// The minimum exception level to display
        /// </summary>
        // TODO: Add debug setting
        public override ExceptionLevel DisplayExceptionLevel => ExceptionLevel.Error;

        /// <summary>
        /// Optional method to run when setting up a Window
        /// </summary>
        /// <param name="window">The Window which is being set up</param>
        public override void OnWindowSetup(Window window)
        {
            // Run base setup
            base.OnWindowSetup(window);

            // TODO: Add path
            // Set localization source
            //ResxExtension.SetDefaultResxName(window, "");
        }
    }
}
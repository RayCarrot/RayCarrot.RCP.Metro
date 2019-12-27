using System.Windows;
using Infralution.Localization.Wpf;
using RayCarrot.RCP.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The API controller UI settings for the Rayman Control Panel Metro
    /// </summary>
    public class RCPMetroAPIControllerUISettings : APIControllerUISettings
    {
        /// <summary>
        /// The application base path
        /// </summary>
        public override string ApplicationBasePath => "pack://application:,,,/RayCarrot.RCP.Metro;component/";

        /// <summary>
        /// Indicates if Window transitions are enabled
        /// </summary>
        public override bool AreWindowTransitionsEnabled => RCFRCP.Data?.EnableAnimations ?? true;

        /// <summary>
        /// Optional method to run when setting up a Window
        /// </summary>
        /// <param name="window">The Window which is being set up</param>
        public override void OnWindowSetup(Window window)
        {
            // Run base setup
            base.OnWindowSetup(window);

            // Set localization source
            ResxExtension.SetDefaultResxName(window, AppLanguages.ResourcePath);
        }
    }
}
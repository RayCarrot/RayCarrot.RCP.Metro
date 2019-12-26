using RayCarrot.RCP.Core.UI;

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
    }
}
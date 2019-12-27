using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.RCP.UI;

namespace RayCarrot.RCP.UI
{
    /// <summary>
    /// Shortcuts for the Carrot Framework
    /// </summary>
    public static class RCFRCPUI
    {
        /// <summary>
        /// The app view model
        /// </summary>
        public static BaseRCPAppViewModel App => RCF.GetService<BaseRCPAppViewModel>();

        /// <summary>
        /// The localization manager
        /// </summary>
        public static RCPLocalizationManager Localization => RCF.GetService<RCPLocalizationManager>();
    }
}
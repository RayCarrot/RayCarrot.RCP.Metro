using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.RCP.UI;
using RayCarrot.UserData;

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

        /// <summary>
        /// The application user data
        /// </summary>
        public static RCPAppUserData Data => RCFData.UserDataCollection.GetUserData("AppUserData") as RCPAppUserData;
    }
}
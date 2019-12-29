using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UserData;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Shortcuts for the Carrot Framework
    /// </summary>
    public static class RCFRCPC
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

        /// <summary>
        /// The API controller manager
        /// </summary>
        public static IAPIControllerManager API => RCF.GetService<IAPIControllerManager>();

        /// <summary>
        /// The update manager
        /// </summary>
        public static IUpdaterManager UpdaterManager => RCF.GetService<IUpdaterManager>();

        /// <summary>
        /// The file manager
        /// </summary>
        public static IFileManager File => RCF.GetService<IFileManager>();

        /// <summary>
        /// The application paths
        /// </summary>
        public static RCPApplicationPaths Path => RCF.GetService<RCPApplicationPaths>();
    }
}
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.RCP.Core;
using RayCarrot.UI;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;

[assembly: RCFDefaultService(typeof(IExceptionHandler), typeof(RCPExceptionHandler), true)]
[assembly: RCFDefaultService(typeof(IMessageUIManager), typeof(RCPMessageUIManager), true)]
[assembly: RCFDefaultService(typeof(IBrowseUIManager), typeof(DefaultWPFBrowseUIManager), true)]
[assembly: RCFDefaultService(typeof(IRegistryManager), typeof(DefaultRegistryManager), true)]
[assembly: RCFDefaultService(typeof(IFileManager), typeof(RCPFileManager), true)]
[assembly: RCFDefaultService(typeof(IDialogBaseManager), typeof(RCPDialogBaseManager), true)]
[assembly: RCFDefaultService(typeof(IUpdaterManager), typeof(RCPUpdateManager), true)]
[assembly: RCFDefaultService(typeof(RCPApplicationPaths), typeof(RCPApplicationPaths), true)]

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
        public static BaseAPIControllerManager API => RCF.GetService<BaseAPIControllerManager>();

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
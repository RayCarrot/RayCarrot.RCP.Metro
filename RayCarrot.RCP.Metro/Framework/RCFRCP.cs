using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.RCP.Core;
using RayCarrot.UserData;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Shortcuts for the Carrot Framework
    /// </summary>
    public static class RCFRCP
    {
        /// <summary>
        /// The application user data
        /// </summary>
        public static AppUserData Data => RCFData.UserDataCollection.GetUserData<AppUserData>();

        /// <summary>
        /// The app view model
        /// </summary>
        public static AppViewModel App => RCFRCPC.App.CastTo<AppViewModel>();

        /// <summary>
        /// The App UI manager
        /// </summary>
        public static AppUIManager UI => RCF.GetService<AppUIManager>();

        /// <summary>
        /// The backup manager
        /// </summary>
        public static BackupManager Backup => RCF.GetService<BackupManager>();

        /// <summary>
        /// The application paths
        /// </summary>
        public static RCPMetroApplicationPaths Path => RCFRCPC.Path.CastTo<RCPMetroApplicationPaths>();
    }
}
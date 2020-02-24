using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
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
        public static AppViewModel App => RCF.GetService<AppViewModel>();

        /// <summary>
        /// The App UI manager
        /// </summary>
        public static AppUIManager UI => RCF.GetService<AppUIManager>();

        /// <summary>
        /// The backup manager
        /// </summary>
        public static BackupManager Backup => RCF.GetService<BackupManager>();

        /// <summary>
        /// The update manager
        /// </summary>
        public static IUpdaterManager UpdaterManager => RCF.GetService<IUpdaterManager>();

        /// <summary>
        /// The file manager
        /// </summary>
        public static IFileManager File => RCFIO.File;
    }
}
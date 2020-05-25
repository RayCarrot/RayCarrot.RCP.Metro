using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Shortcuts for the common RCP application services
    /// </summary>
    public static class RCPServices
    {
        /// <summary>
        /// The application user data
        /// </summary>
        public static AppUserData Data => BaseApp.Current.GetService<AppUserData>();

        /// <summary>
        /// The app view model
        /// </summary>
        public static AppViewModel App => BaseApp.Current.GetService<AppViewModel>();

        /// <summary>
        /// The App UI manager
        /// </summary>
        public static AppUIManager UI => BaseApp.Current.GetService<AppUIManager>();

        /// <summary>
        /// The backup manager
        /// </summary>
        public static BackupManager Backup => BaseApp.Current.GetService<BackupManager>();

        /// <summary>
        /// The update manager
        /// </summary>
        public static IUpdaterManager UpdaterManager => BaseApp.Current.GetService<IUpdaterManager>();

        /// <summary>
        /// The file manager
        /// </summary>
        public static IFileManager File => BaseApp.Current.GetService<IFileManager>();
    }
}
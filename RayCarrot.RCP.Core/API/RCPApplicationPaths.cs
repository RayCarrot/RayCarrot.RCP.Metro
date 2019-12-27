using System;
using RayCarrot.IO;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// The base class for Rayman Control Panel application paths
    /// </summary>
    public abstract class RCPApplicationPaths
    {
        /// <summary>
        /// The code name for this application to use for the application paths
        /// </summary>
        public abstract string AppName { get; }

        /// <summary>
        /// The base user data directory for the Rayman Control Panel
        /// </summary>
        public FileSystemPath UserDataBaseDir => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman Control Panel";

        /// <summary>
        /// The base user data directory for the current application
        /// </summary>
        public FileSystemPath AppUserDataBaseDir => UserDataBaseDir + AppName;

        /// <summary>
        /// The user data directory for temporary data
        /// </summary>
        public FileSystemPath TempDir => AppUserDataBaseDir + "Temp";

        /// <summary>
        /// The user data directory for temporary installation data
        /// </summary>
        public FileSystemPath TempInstallationDir => TempDir + "Installation";

        /// <summary>
        /// The log file
        /// </summary>
        public FileSystemPath LogFile => TempDir + "Log.txt";

        /// <summary>
        /// The updater file
        /// </summary>
        public FileSystemPath UpdaterFile => TempInstallationDir + "Updater.exe";

        /// <summary>
        /// Gets the user data file
        /// </summary>
        /// <param name="userDataType">The type of user data to get the file for</param>
        /// <returns>The file path</returns>
        public FileSystemPath GetUserDataFile(Type userDataType) => AppUserDataBaseDir + $"{userDataType.Name.ToLower()}.json";
    }
}
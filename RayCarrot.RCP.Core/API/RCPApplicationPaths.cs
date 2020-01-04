using System;
using RayCarrot.IO;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// The base class for Rayman Control Panel application paths
    /// </summary>
    public class RCPApplicationPaths
    {
        /// <summary>
        /// The code name for this application to use for the application paths
        /// </summary>
        protected string AppName => RCFRCPC.API.AppCodeName;

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
        /// The base utilities directory
        /// </summary>
        public FileSystemPath UtilitiesBaseDir => AppUserDataBaseDir + "Utilities";

        /// <summary>
        /// The common path to the ubi.ini file
        /// </summary>
        public FileSystemPath UbiIniPath1 => Environment.SpecialFolder.Windows.GetFolderPath() + @"Ubisoft\ubi.ini";

        /// <summary>
        /// The second common path to the ubi.ini file
        /// </summary>
        public FileSystemPath UbiIniPath2 => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "VirtualStore\\Windows\\Ubisoft\\Ubi.ini";

        /// <summary>
        /// Gets the user data file
        /// </summary>
        /// <param name="userDataType">The type of user data to get the file for</param>
        /// <returns>The file path</returns>
        public FileSystemPath GetUserDataFile(Type userDataType) => AppUserDataBaseDir + $"{userDataType.Name.ToLower()}.json";

        /// <summary>
        /// The update manifest URL
        /// </summary>
        public string UpdateManifestUrl => BaseUrl + $"{AppName}_Manifest.json";

        /// <summary>
        /// The base URL
        /// </summary>
        public const string BaseUrl = "http://raycarrot.ylemnova.com/RCP/";
    }
}
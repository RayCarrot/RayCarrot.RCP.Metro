using RayCarrot.IO;
using RayCarrot.RCP.Core;
using System;
using RayCarrot.Windows.Registry;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The application paths for the Rayman Control Panel Metro
    /// </summary>
    public class RCPMetroApplicationPaths : RCPApplicationPaths
    {
        /// <summary>
        /// The code name for this application to use for the application paths
        /// </summary>
        public override string AppName => "RCP_Metro";

        /// <summary>
        /// The base utilities directory
        /// </summary>
        public FileSystemPath UtilitiesBaseDir => AppUserDataBaseDir + "Utilities";

        /// <summary>
        /// The base games directory
        /// </summary>
        public FileSystemPath GamesBaseDir => AppUserDataBaseDir + "Games";

        /// <summary>
        /// The TPLS directory
        /// </summary>
        public FileSystemPath TPLSDir => UtilitiesBaseDir + "TPLS";

        /// <summary>
        /// The uninstaller file path
        /// </summary>
        public FileSystemPath UninstallFilePath => TempInstallationDir + "Uninstaller.exe";

        /// <summary>
        /// The admin worker file path
        /// </summary>
        public FileSystemPath AdminWorkerPath => TempDir + "AdditionalFiles\\Rayman Control Panel - Admin Worker.exe";

        /// <summary>
        /// The common path to the ubi.ini file
        /// </summary>
        public FileSystemPath UbiIniPath1 => @"C:\Windows\Ubisoft\ubi.ini";

        /// <summary>
        /// The second common path to the ubi.ini file
        /// </summary>
        public FileSystemPath UbiIniPath2 => Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "VirtualStore\\Windows\\Ubisoft\\Ubi.ini";

        /// <summary>
        /// The Registry uninstall key name
        /// </summary>
        public string RegistryUninstallKeyName => RCFRCP.Path.AppName;

        /// <summary>
        /// The Rayman Raving Rabbids registry key path
        /// </summary>
        public string RaymanRavingRabbidsRegistryKey => @"HKEY_CURRENT_USER\Software\Ubisoft\Rayman4\{05D2C1BC-A857-4493-9BDA-C7707CACB937}";

        /// <summary>
        /// The Rayman Raving Rabbids 2 registry key path
        /// </summary>
        public string RaymanRavingRabbids2RegistryKey => @"HKEY_CURRENT_USER\Software\Ubisoft\Rayman Raving Rabbids 2";

        /// <summary>
        /// The Rayman Origins registry key path
        /// </summary>
        public string RaymanOriginsRegistryKey => @"HKEY_CURRENT_USER\Software\Ubisoft\RaymanOrigins";

        /// <summary>
        /// The Rayman Legends registry key path
        /// </summary>
        public string RaymanLegendsRegistryKey => @"HKEY_CURRENT_USER\Software\Ubisoft\Rayman Legends";

        /// <summary>
        /// The file extension for compressed backups
        /// </summary>
        public string BackupCompressionExtension => ".rcpb";

        #region Public Constant Values
            
        /// <summary>
        /// The Registry base key
        /// </summary>
        public const string RegistryBaseKey = RCFRegistryPaths.BasePath + "\\RCP_Metro";

        /// <summary>
        /// The license accepted value name
        /// </summary>
        public const string RegistryLicenseValue = "LicenseAccepted";

        #endregion
    }

}
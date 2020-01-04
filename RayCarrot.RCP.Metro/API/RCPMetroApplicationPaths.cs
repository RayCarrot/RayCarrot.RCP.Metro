using RayCarrot.IO;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The application paths for the Rayman Control Panel Metro
    /// </summary>
    public class RCPMetroApplicationPaths : RCPApplicationPaths
    {
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
    }
}
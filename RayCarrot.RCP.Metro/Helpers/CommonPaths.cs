using System;
using System.IO;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Common paths used in the Rayman Control Panel
    /// </summary>
    public static class CommonPaths
    {
        /// <summary>
        /// The base user data directory
        /// </summary>
        public static FileSystemPath UserDataBaseDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rayman Control Panel", "3.0");

        /// <summary>
        /// The <see cref="AppUserData"/> file path
        /// </summary>
        public static FileSystemPath AppUserDataPath => UserDataBaseDir + "appuserdata.json";

        /// <summary>
        /// The path for temporary files in this application
        /// </summary>
        public static FileSystemPath TempPath => Path.Combine(Path.GetTempPath(), "Rayman Control Panel 3.0");

        /// <summary>
        /// The common path to the ubi.ini file
        /// </summary>
        public static FileSystemPath UbiIniPath1 => @"C:\Windows\Ubisoft\ubi.ini";

        /// <summary>
        /// The second common path to the ubi.ini file
        /// </summary>
        public static FileSystemPath UbiIniPath2 => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore\\Windows\\Ubisoft\\Ubi.ini");

        /// <summary>
        /// The registry base key
        /// </summary>
        public const string RegistryBaseKey = RCFRegistryPaths.BasePath + @"\RCP_Metro";

        /// <summary>
        /// The license accepted value name
        /// </summary>
        public static string RegistryLicenseValue => "LicenseAccepted";
    }
}
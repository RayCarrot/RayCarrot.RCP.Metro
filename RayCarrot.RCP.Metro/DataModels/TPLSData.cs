using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// TPLS data for the Rayman 1 utility
    /// </summary>
    public class TPLSData : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="installDir">The directory it is installed under</param>
        public TPLSData(FileSystemPath installDir)
        {
            InstallDir = installDir;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The directory it is installed under
        /// </summary>
        public FileSystemPath InstallDir { get; }

        /// <summary>
        /// The selected Rayman version to search for
        /// </summary>
        public TPLSRaymanVersion RaymanVersion { get; set; }

        /// <summary>
        /// The selected DosBox version to search for
        /// </summary>
        public TPLSDOSBoxVersion DosBoxVersion { get; set; }

        /// <summary>
        /// Indicates if the utility is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        #endregion
    }
}
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Options for a DOSBox game
    /// </summary>
    public class UserData_DosBoxOptions
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserData_DosBoxOptions()
        {
            MountPath = FileSystemPath.EmptyPath;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The file or directory to mount
        /// </summary>
        public FileSystemPath MountPath { get; set; }

        #endregion
    }
}
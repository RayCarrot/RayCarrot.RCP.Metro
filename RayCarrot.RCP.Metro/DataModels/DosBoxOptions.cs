using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Options for a DosBox game
    /// </summary>
    public class DosBoxOptions
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DosBoxOptions()
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
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The result for the <see cref="BaseFinderItem.CustomFinderAction"/>
    /// </summary>
    public class FoundActionResult
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="installDir">The install directory</param>
        /// <param name="parameter">Optional parameter when the item is handled</param>
        public FoundActionResult(FileSystemPath installDir, object parameter)
        {
            InstallDir = installDir;
            Parameter = parameter;
        }

        /// <summary>
        /// The install directory
        /// </summary>
        public FileSystemPath InstallDir { get; }

        /// <summary>
        /// Optional parameter when the item is handled
        /// </summary>
        public object Parameter { get; }
    }
}
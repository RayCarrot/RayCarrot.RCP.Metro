using ByteSizeLib;
using RayCarrot.IO;
using RayCarrot.UI;
using System.IO;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a drive
    /// </summary>
    public class DriveViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// The drive icon
        /// </summary>
        public ImageSource Icon { get; set; }

        /// <summary>
        /// The drive label
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The drive root path
        /// </summary>
        public FileSystemPath Path { get; set; }

        /// <summary>
        /// The drive format
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// The available free space
        /// </summary>
        public ByteSize? FreeSpace { get; set; }

        /// <summary>
        /// The total size
        /// </summary>
        public ByteSize? TotalSize { get; set; }

        /// <summary>
        /// The type of drive
        /// </summary>
        public DriveType? Type { get; set; }

        /// <summary>
        /// True if the drive is ready, false if not or null if the check failed
        /// </summary>
        public bool? IsReady { get; set; }

        #endregion

        #region Public Display Properties

        /// <summary>
        /// The display text for the drive label
        /// </summary>
        public string DisplayLabel => Label ?? "{Error reading label}";

        /// <summary>
        /// The display text for the drive format
        /// </summary>
        public string DisplayFormat => Format ?? "{Error reading format}";

        /// <summary>
        /// The display text for the available free space
        /// </summary>
        public string DisplayFreeSpace => FreeSpace?.ToString() ?? "{Error reading free space}";

        /// <summary>
        /// The display text for the total size
        /// </summary>
        public string DisplayTotalSize => TotalSize?.ToString() ?? "{Error reading total size}";

        /// <summary>
        /// The display text for the drive type
        /// </summary>
        public string DisplayType => Type?.ToString() ?? "{Error reading type}";

        /// <summary>
        /// The display text for the drive ready status
        /// </summary>
        public string DisplayIsReady => IsReady == true ? "Yes" : "No";

        #endregion
    }
}
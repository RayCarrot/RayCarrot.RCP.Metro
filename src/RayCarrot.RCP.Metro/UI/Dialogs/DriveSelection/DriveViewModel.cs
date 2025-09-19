using System.IO;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a drive
/// </summary>
public class DriveViewModel : BaseViewModel
{
    /// <summary>
    /// The drive icon
    /// </summary>
    public ImageSource Icon { get; init; }

    /// <summary>
    /// The drive root path
    /// </summary>
    public FileSystemPath Path { get; init; }

    /// <summary>
    /// The drive label
    /// </summary>
    public string Label { get; init; }

    /// <summary>
    /// The available free space
    /// </summary>
    public long? FreeSpace { get; init; }
    public string FreeSpaceDisplayString => FreeSpace == null ? String.Empty : BinaryHelpers.BytesToString(FreeSpace.Value);

    /// <summary>
    /// The used space
    /// </summary>
    public long? UsedSpace => TotalSize - FreeSpace;
    public string UsedSpaceDisplayString => UsedSpace == null ? String.Empty : BinaryHelpers.BytesToString(UsedSpace.Value);

    /// <summary>
    /// The total size
    /// </summary>
    public long? TotalSize { get; init; }
    public string TotalSizeDisplayString => TotalSize == null ? String.Empty : BinaryHelpers.BytesToString(TotalSize.Value);

    /// <summary>
    /// The drive format
    /// </summary>
    public string Format { get; init; }

    /// <summary>
    /// The type of drive
    /// </summary>
    public DriveType? Type { get; init; }

    /// <summary>
    /// True if the drive is ready, false if not or null if the check failed
    /// </summary>
    public bool? IsReady { get; init; }
}
#nullable disable
using System.Collections.Generic;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The result when browsing for a drive
/// </summary>
public class DriveBrowserResult : UserInputResult
{
    /// <summary>
    /// The selected drive
    /// </summary>
    public FileSystemPath SelectedDrive { get; set; }

    /// <summary>
    /// The selected drives
    /// </summary>
    public IEnumerable<FileSystemPath> SelectedDrives { get; set; }
}
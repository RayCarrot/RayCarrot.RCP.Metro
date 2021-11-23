using System.Collections.Generic;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The result when browsing for a directory
/// </summary>
public class DirectoryBrowserResult : UserInputResult
{
    /// <summary>
    /// The selected directory
    /// </summary>
    public FileSystemPath SelectedDirectory { get; set; }

    /// <summary>
    /// The list of selected directories
    /// </summary>
    public IEnumerable<FileSystemPath> SelectedDirectories { get; set; }
}
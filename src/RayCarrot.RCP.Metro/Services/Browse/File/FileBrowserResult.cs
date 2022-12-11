#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The result when browsing for a directory
/// </summary>
public class FileBrowserResult : UserInputResult
{
    /// <summary>
    /// The selected file
    /// </summary>
    public FileSystemPath SelectedFile { get; set; }

    /// <summary>
    /// The list of selected files
    /// </summary>
    public FileSystemPath[] SelectedFiles { get; set; }
}
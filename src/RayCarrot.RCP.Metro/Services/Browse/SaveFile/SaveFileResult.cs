#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The result when saving a file
/// </summary>
public class SaveFileResult : UserInputResult
{
    /// <summary>
    /// The selected location to save the file
    /// </summary>
    public FileSystemPath SelectedFileLocation { get; set; }
}
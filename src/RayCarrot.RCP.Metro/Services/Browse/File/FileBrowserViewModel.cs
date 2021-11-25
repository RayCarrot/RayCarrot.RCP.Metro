#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// A model to use when browsing for a file
/// </summary>
public class FileBrowserViewModel : BaseBrowseViewModel
{
    /// <summary>
    /// The filter to use for file extensions
    /// </summary>
    public string ExtensionFilter { get; set; }

    /// <summary>
    /// Enables or disables multi selection option
    /// </summary>
    public bool MultiSelection { get; set; }
}
#nullable disable
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a file extension selection dialog
/// </summary>
public class FileExtensionSelectionDialogViewModel : UserInputViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="fileFormats">The available file formats</param>
    /// <param name="header">The header to display</param>
    public FileExtensionSelectionDialogViewModel(string[] fileFormats, string header)
    {
        // Set properties
        FileFormats = fileFormats;
        Header = header;
        SelectedFileFormat = FileFormats.First();

        // Set the default title
        Title = Resources.Archive_FileExtensionSelectionHeader;
    }

    /// <summary>
    /// The header to display
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// The available file formats
    /// </summary>
    public string[] FileFormats { get; }

    /// <summary>
    /// The selected file format
    /// </summary>
    public string SelectedFileFormat { get; set; }
}
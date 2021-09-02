using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The result for a file extension selection dialog
    /// </summary>
    public class FileExtensionSelectionDialogResult : UserInputResult
    {
        /// <summary>
        /// The selected file format
        /// </summary>
        public string SelectedFileFormat { get; set; }
    }
}
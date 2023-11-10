namespace RayCarrot.RCP.Metro;

/// <summary>
/// The result for an item selection dialog
/// </summary>
public class ItemSelectionDialogResult : UserInputResult
{
    /// <summary>
    /// The selected item index
    /// </summary>
    public int SelectedIndex { get; set; }
}
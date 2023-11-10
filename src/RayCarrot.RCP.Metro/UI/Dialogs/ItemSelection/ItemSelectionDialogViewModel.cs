namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for an item selection dialog
/// </summary>
public class ItemSelectionDialogViewModel : UserInputViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="items">The available items</param>
    /// <param name="header">The header to display</param>
    public ItemSelectionDialogViewModel(string[] items, string header)
    {
        // Set properties
        Items = items;
        Header = header;
        SelectedIndex = 0;

        // Set the default title
        Title = Resources.Archive_FileExtensionSelectionHeader;
    }

    /// <summary>
    /// The header to display
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// The available items
    /// </summary>
    public string[] Items { get; }

    /// <summary>
    /// The selected item index
    /// </summary>
    public int SelectedIndex { get; set; }
}
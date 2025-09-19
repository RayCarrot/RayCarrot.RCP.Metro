namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a string input dialog
/// </summary>
public class StringInputViewModel : UserInputViewModel
{
    /// <summary>
    /// The header
    /// </summary>
    public string HeaderText { get; set; }

    /// <summary>
    /// The string input by the user
    /// </summary>
    public string StringInput { get; set; }
}
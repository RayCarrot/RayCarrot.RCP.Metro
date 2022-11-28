using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base for a page control
/// </summary>
public class BasePage : UserControl
{
    /// <summary>
    /// The overflow menu
    /// </summary>
    public object? PopupMenu { get; set; }
}
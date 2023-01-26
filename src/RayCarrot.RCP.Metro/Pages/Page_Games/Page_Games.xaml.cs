using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for Page_Games.xaml
/// </summary>
public partial class Page_Games : BasePage
{
    public Page_Games()
    {
        InitializeComponent();
    }

    private void Page_Games_OnKeyDown(object sender, KeyEventArgs e)
    {
        // Auto-focus search text box when you start typing
        GameSelectionControl.SearchTextBox.Focus();
    }
}
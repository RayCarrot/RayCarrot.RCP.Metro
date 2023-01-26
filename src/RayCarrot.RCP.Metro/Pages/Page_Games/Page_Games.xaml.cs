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
        if (e.Key is Key.Enter or Key.Up or Key.Down or Key.Left or Key.Right)
            return;

        // Auto-focus search text box when you start typing
        GameSelectionControl.SearchTextBox.Focus();
    }
}
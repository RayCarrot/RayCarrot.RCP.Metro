using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Games;

/// <summary>
/// Interaction logic for Page_Games.xaml
/// </summary>
public partial class GamesPage : BasePage
{
    public GamesPage()
    {
        InitializeComponent();
    }

    private void Page_Games_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.IsKeyDown(Key.Escape))
            return;

        if (e.Key is Key.Enter or Key.Up or Key.Down or Key.Left or Key.Right)
            return;

        // Auto-focus search text box when you start typing
        GameSelectionControl.SearchTextBox.Focus();
    }
}
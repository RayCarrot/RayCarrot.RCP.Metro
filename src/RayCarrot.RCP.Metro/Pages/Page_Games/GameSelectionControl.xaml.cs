using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameSelectionControl.xaml
/// </summary>
public partial class GameSelectionControl : UserControl
{
    public GameSelectionControl()
    {
        InitializeComponent();
    }

    public Page_Games_ViewModel ViewModel => (Page_Games_ViewModel)DataContext;

    private void GamesListBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Redirect the mouse wheel movement to allow scrolling
        MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = e.Source
        };

        GamesPanelScrollViewer?.RaiseEvent(eventArg);
        e.Handled = true;
    }

    private void GameGroupListBoxItem_OnSelected(object sender, RoutedEventArgs e)
    {
        if (sender is not ListBoxItem { DataContext: InstalledGameViewModel vm } item)
            return;

        // Scroll the item into view
        item.BringIntoView();

        // Select the item in the main view model
        ViewModel.SelectedInstalledGame = vm;

        // Deselect other games
        foreach (InstalledGameCategoryViewModel gameCategoryViewModel in ViewModel.GameCategories)
        {
            foreach (InstalledGameGroupViewModel gameGroupViewModel in gameCategoryViewModel.GameGroups)
            {
                if (gameGroupViewModel.SelectedInstalledGame != vm)
                    gameGroupViewModel.SelectedInstalledGame = null;
            }
        }
    }
}
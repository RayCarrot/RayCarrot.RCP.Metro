using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Games;

/// <summary>
/// Interaction logic for GameSelectionControl.xaml
/// </summary>
public partial class GameSelectionControl : UserControl
{
    public GameSelectionControl()
    {
        InitializeComponent();
    }

    public GamesPageViewModel ViewModel => (GamesPageViewModel)DataContext;

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

        // Ignore if already selected
        if (ViewModel.SelectedInstalledGame == vm)
            return;

        // Scroll the item into view
        item.BringIntoView();

        // Focus on the item
        item.Focus();

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

    private void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            InstalledGameGroupViewModel? firstGameGroup = ViewModel.FilteredGameCategories.
                Cast<InstalledGameCategoryViewModel>().
                FirstOrDefault()?.FilteredGameGroups.
                Cast<InstalledGameGroupViewModel>().
                FirstOrDefault();

            if (firstGameGroup != null)
                firstGameGroup.IsSelected = true;

            e.Handled = true;
        }
    }
}
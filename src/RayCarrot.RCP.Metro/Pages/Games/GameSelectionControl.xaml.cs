using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

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

    private void GamesListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        DragDrop.SetDropHandler(GamesListBox, new GameSelectionDropHandler(ViewModel));
    }

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

    private void GamesListBoxItem_OnSelected(object sender, RoutedEventArgs e)
    {
        if (sender is not ListBoxItem item)
            return;

        // Scroll the item into view
        item.BringIntoView();

        // Focus on the item
        item.Focus();
    }

    private void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            InstalledGameViewModel? firstGameInstallation = ViewModel.GamesView.
                Cast<InstalledGameViewModel>().
                FirstOrDefault();

            if (firstGameInstallation != null)
                ViewModel.SelectedInstalledGame = firstGameInstallation;

            e.Handled = true;
        }
    }
}
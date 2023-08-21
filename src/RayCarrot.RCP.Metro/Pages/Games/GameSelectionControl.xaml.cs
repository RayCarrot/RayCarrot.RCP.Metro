using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop.Utilities;
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

    private ScrollViewer? _gamesScrollViewer;

    public GamesPageViewModel ViewModel => (GamesPageViewModel)DataContext;

    private void GamesListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        DragDrop.SetDropHandler(GamesListBox, new GameSelectionDropHandler(ViewModel));
        _gamesScrollViewer = GamesListBox.GetDescendantByType<ScrollViewer>();
    }

    private void GamesListBoxItem_OnSelected(object sender, RoutedEventArgs e)
    {
        if (sender is not ListBoxItem item)
            return;

        GroupItem group = item.GetVisualAncestor<GroupItem>();
        group.BringIntoView();
        group.Focus();
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

    private void GamesListBox_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (DataContext == null || _gamesScrollViewer == null || !ViewModel.ShowGameCategories)
            return;

        foreach (GameCategoryViewModel gamesCategory in ViewModel.GamesCategories)
            gamesCategory.IsSelected = false;

        // Find the first visible game
        InstalledGameViewModel? game = GamesListBox.Items.
            Cast<InstalledGameViewModel>().
            FirstOrDefault(x => ((ListBoxItem)GamesListBox.ItemContainerGenerator.ContainerFromItem(x)).IsUserVisible(GamesListBox));

        if (game == null)
            return;

        // Select the category for that game
        game.GameCategory.IsSelected = true;

        // If we're scrolled to the bottom we also select all subsequent categories
        if (Math.Abs(_gamesScrollViewer.VerticalOffset - _gamesScrollViewer.ScrollableHeight) < 1.0)
        {
            bool foundCategory = false;

            foreach (GameCategoryViewModel category in ViewModel.GamesCategories)
            {
                if (!foundCategory)
                {
                    if (category == game.GameCategory)
                        foundCategory = true;
                }
                else if (category.IsEnabled)
                {
                    category.IsSelected = true;
                }
            }
        }
    }

    private void GameCategoryButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: GameCategoryViewModel gameCategory } ||
            _gamesScrollViewer == null ||
            !ViewModel.ShowGameCategories) 
            return;

        // Find the first game in the category
        InstalledGameViewModel? game = GamesListBox.Items.
            Cast<InstalledGameViewModel>().
            FirstOrDefault(x => x.GameDescriptor.Category == gameCategory.Category);

        if (game == null)
            return;

        // Scroll all the way down and then up. This places the item we scroll to on the top rather than the bottom.
        _gamesScrollViewer?.ScrollToBottom();

        ViewModel.SelectedInstalledGame = game;
    }

    private void GameGroupRadioButton_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Allow game groups to be selected on mouse down rather than waiting for mouse up
        if (sender is RadioButton radioButton && 
            // Make sure we didn't click on a game directly
            e.OriginalSource is DependencyObject d && d.FindParent<ListBoxItem>() == null)
        {
            radioButton.IsChecked = true;
        }
    }
}
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.Pages.Games;

/// <summary>
/// Interaction logic for GamesHomeControl.xaml
/// </summary>
public partial class GamesHomeControl : UserControl
{
    public GamesHomeControl()
    {
        InitializeComponent();

        Loaded += GamesHomeControl_OnLoaded;
    }

    private const int GameItemSize = 100 + 4 + 4; // 100 in width, 4 in left and right margin

    private void GamesHomeControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= GamesHomeControl_OnLoaded;

        if (DataContext is GamesPageViewModel vm)
        {
            RefreshRecentGamesHeight();

            vm.PropertyChanged += (_, ee) =>
            {
                if (ee.PropertyName == nameof(GamesPageViewModel.ShowFavoriteGames))
                {
                    RefreshRecentGamesHeight();
                }
            };
        }
    }

    private void RecentGamesStackPanel_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Cut the width down to not show part of an item in the panel
        double width = e.NewSize.Width;

        width = (int)(width / GameItemSize) * GameItemSize;
        RecentGamesItemsControl.Width = width;
    }

    private void RefreshRecentGamesHeight()
    {
        // If we show favorite games then we limit the height to one row
        if (DataContext is GamesPageViewModel vm)
            RecentGamesItemsControl.Height = vm.ShowFavoriteGames ? GameItemSize : Double.NaN;
    }
}
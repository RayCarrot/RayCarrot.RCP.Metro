using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameHubControl.xaml
/// </summary>
public partial class GameHubControl : UserControl
{
    public GameHubControl()
    {
        InitializeComponent();
    }

    private void RootScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer sv)
            return;

        // Scroll banner image by a 1/3 of the scroll speed for a nice parallax effect
        GameBannerImage.Margin = new Thickness(0, -(sv.VerticalOffset / 3), 0, sv.VerticalOffset / 3);

        const int topOffset = 25;
        double imgHeight = GameIcon.ActualHeight;
        double imgHalfHeight = imgHeight / 2;
        double iconYPos = GameIcon.TransformToAncestor(this).
            Transform(new Point(0, 0)).Y - GameIcon.Margin.Top;
        double iconScrollFactor = (-iconYPos).Clamp(0, imgHalfHeight) / imgHalfHeight;

        Thickness selectedGameGameIconMargin = GameIcon.Margin;
        selectedGameGameIconMargin.Top = iconScrollFactor * (imgHalfHeight + topOffset);
        GameIcon.Margin = selectedGameGameIconMargin;

        Thickness selectedGameTopBarActionsGridMargin = TopBarActionsGrid.Margin;
        selectedGameTopBarActionsGridMargin.Top = iconScrollFactor * topOffset;
        TopBarActionsGrid.Margin = selectedGameTopBarActionsGridMargin;
    }

    private void GamePanelsUniformGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not UniformGrid grid)
            return;

        const int minPanelWidth = 300;

        grid.Columns = (int)(grid.ActualWidth / minPanelWidth).Clamp(1, grid.Children.Count);
    }

    private void DropDownButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Toggle the state of the popup
        DropDownPopup.IsOpen ^= true;
    }
}
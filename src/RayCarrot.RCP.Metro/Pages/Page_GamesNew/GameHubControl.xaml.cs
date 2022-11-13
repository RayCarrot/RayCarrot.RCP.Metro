using System;
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

        // Scroll background border. The reason we have it outside of the normal
        // scroll viewer is so that it can appear behind the scroll bar thumb
        BackgroundBorder.Margin = new Thickness(0, -sv.VerticalOffset, 0, sv.VerticalOffset);

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
        if (sender is UniformGrid grid) 
            UpdateGamePanelsUniformGrid(grid);
    }

    private void GamePanelsUniformGrid_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is UniformGrid grid)
            UpdateGamePanelsUniformGrid(grid);
    }

    private static void UpdateGamePanelsUniformGrid(UniformGrid grid)
    {
        const int minPanelWidth = 300;
        int minColumns = grid.ActualWidth < (minPanelWidth * 2) ? 1 : 2;
        grid.Columns = (int)(grid.ActualWidth / minPanelWidth).Clamp(minColumns, Math.Max(minColumns, grid.Children.Count));
    }
}
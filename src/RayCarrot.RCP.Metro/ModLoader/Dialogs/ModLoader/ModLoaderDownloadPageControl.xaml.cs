using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

/// <summary>
/// Interaction logic for ModLoaderDownloadPageControl.xaml
/// </summary>
public partial class ModLoaderDownloadPageControl : UserControl
{
    public ModLoaderDownloadPageControl()
    {
        InitializeComponent();
    }

    private ScrollViewer? ModsScrollViewer { get; set; }

    public DownloadableModsViewModel ViewModel => (DownloadableModsViewModel)DataContext;

    private void ModsGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r?.VisualHit.GetType() != typeof(ListBoxItem))
        {
            ViewModel.SelectedMod = null;
        }
    }

    private void ModsListBox_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is Visual obj)
            ModsScrollViewer = obj.GetDescendantByType<ScrollViewer>();
    }

    private async void ModsScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // Load the next chunk of pages when scrolled to the bottom
        if (ModsScrollViewer != null &&
            ViewModel.ModsFeed.CanLoadChunk &&
            ModsScrollViewer.ScrollableHeight > 0 &&
            ModsScrollViewer.VerticalOffset >= ModsScrollViewer.ScrollableHeight)
        {
            await ViewModel.LoadNextChunkAsync();
        }
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        ModsScrollViewer?.ScrollToTop();
    }
}
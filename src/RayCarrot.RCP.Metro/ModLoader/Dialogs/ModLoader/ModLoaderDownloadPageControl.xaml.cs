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

    public DownloadableModsViewModel ViewModel => (DownloadableModsViewModel)DataContext;

    private void ModsGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r.VisualHit.GetType() != typeof(ListBoxItem))
        {
            ViewModel.SelectedMod = null;
        }
    }

    private void ModsListBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Redirect the mouse wheel movement to allow scrolling
        MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = e.Source
        };

        ModsScrollViewer?.RaiseEvent(eventArg);
        e.Handled = true;
    }

    private async void ModsScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // Load the next chunk of pages when scrolled to the bottom
        if (ViewModel.CanLoadChunk &&
            ModsScrollViewer.ScrollableHeight > 0 &&
            ModsScrollViewer.VerticalOffset >= ModsScrollViewer.ScrollableHeight)
        {
            await ViewModel.LoadNextChunkAsync();
        }
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        ModsScrollViewer.ScrollToTop();
    }
}
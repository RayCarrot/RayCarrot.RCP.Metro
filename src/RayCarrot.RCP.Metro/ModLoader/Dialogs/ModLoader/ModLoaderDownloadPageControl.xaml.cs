using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

/// <summary>
/// Interaction logic for ModLoaderDownloadPageControl.xaml
/// </summary>
public partial class ModLoaderDownloadPageControl : UserControl
{
    public ModLoaderDownloadPageControl()
    {
        InitializeComponent();
        DataContextChanged += ModLoaderDownloadPageControl_DataContextChanged;
    }

    private ScrollViewer? ModsScrollViewer { get; set; }

    public DownloadableModsViewModel ViewModel => (DownloadableModsViewModel)DataContext;

    private void ModLoaderDownloadPageControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is not DownloadableModsViewModel viewModel)
            return;

        DataContextChanged -= ModLoaderDownloadPageControl_DataContextChanged;

        viewModel.PropertyChanged += (_, ee) =>
        {
            // Different transition based on if it's transitioning in or out
            if (ee.PropertyName == nameof(viewModel.SelectedMod))
            {
                if (viewModel.SelectedMod == null)
                    SelectedModTransitioningContentControl.Transition = TransitionType.Down;
                else
                    SelectedModTransitioningContentControl.Transition = TransitionType.Up;
            }
        };
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
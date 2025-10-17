using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using RayCarrot.RCP.Metro.ModLoader.Sources;

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
                if (!Services.Data.UI_EnableAnimations)
                    SelectedModTransitioningContentControl.Transition = TransitionType.Normal;
                else if (viewModel.SelectedMod == null)
                    SelectedModTransitioningContentControl.Transition = TransitionType.Down;
                else
                    SelectedModTransitioningContentControl.Transition = TransitionType.Up;
            }
        };
        viewModel.FeedInitialized += (_, _) =>
        {
            ScrollViewer? scrollViewer = ModsItemsControl.GetDescendantByType<ScrollViewer>();
            scrollViewer?.ScrollToTop();
        };
    }

    private async void PlaceholderMod_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PlaceholderDownloadableModViewModel placeholder })
            await ViewModel.LoadNextPageFromPlaceholderAsync(placeholder);
    }
}
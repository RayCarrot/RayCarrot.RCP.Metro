using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

/// <summary>
/// Interaction logic for ModifiedFilesControl.xaml
/// </summary>
public partial class ModifiedFilesControl : UserControl
{
    public ModifiedFilesControl()
    {
        InitializeComponent();
    }

    private void FileTreeHeadersGrid_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Hacky workaround for having the headers be aligned with the table items. Without this
        // the scroll bar thumb will offset it incorrectly.
        FileTreeView.ApplyTemplate();
        ItemsPresenter itemsPresenter = (ItemsPresenter)((ScrollViewer)FileTreeView.Template.FindName("TreeViewScrollViewer", FileTreeView)).Content;

        itemsPresenter.SizeChanged -= FileTreeViewItemsPresenter_OnSizeChanged;
        itemsPresenter.SizeChanged += FileTreeViewItemsPresenter_OnSizeChanged;

        FileTreeHeadersGrid.Width = itemsPresenter.ActualWidth;
    }

    private void FileTreeViewItemsPresenter_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ItemsPresenter control = (ItemsPresenter)sender;
        FileTreeHeadersGrid.Width = control.ActualWidth;
    }
}
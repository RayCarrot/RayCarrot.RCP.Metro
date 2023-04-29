using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// Interaction logic for ArchiveExplorerDialog.xaml
/// </summary>
public partial class ArchiveExplorerDialog : WindowContentControl
{
    #region Constructor
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="vm">The view model</param>
    public ArchiveExplorerDialog(ArchiveExplorerDialogViewModel vm)
    {
        // Set up UI
        InitializeComponent();

        // Set properties
        ViewModel = vm;
        DataContext = ViewModel;

        // Set up remaining things once fully loaded
        Loaded += ArchiveExplorer_Loaded;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The view model
    /// </summary>
    public ArchiveExplorerDialogViewModel ViewModel { get; }

    public override bool IsResizable => true;

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Title = ViewModel.Title;
        WindowInstance.Icon = GenericIconKind.Window_ArchiveExplorer;
        WindowInstance.MinWidth = 600;
        WindowInstance.MinHeight = 400;
        WindowInstance.Width = 925;
        WindowInstance.Height = 600;
    }

    protected override async Task<bool> ClosingAsync()
    {
        if (!await base.ClosingAsync())
            return false;

        // Cancel the closing if an archive is running an operation
        if (ViewModel.LoaderViewModel.IsRunning)
            return false;

        // Ask user if there are pending changes
        if (ViewModel.Archives.Any(x => x.HasModifiedFiles))
            return await Services.MessageUI.DisplayMessageAsync(Metro.Resources.UnsavedChangesQuestion,
                Metro.Resources.UnsavedChangesQuestionHeader, MessageType.Question, true);
        else
            return true;
    }

    #endregion

    #region Public Methods

    public void RefreshSort()
    {
        // Clear existing sort descriptions
        FilesList?.Items.SortDescriptions.Clear();
        DirTreeView?.Items.SortDescriptions.Clear();

        switch (Services.Data.Archive_ExplorerSortOption)
        {
            case ArchiveItemsSort.AlphabeticalAscending:
                FilesList?.Items.SortDescriptions.Add(new SortDescription(nameof(FileViewModel.FileName), ListSortDirection.Ascending));
                DirTreeView?.Items.SortDescriptions.Add(new SortDescription(nameof(DirectoryViewModel.DisplayName), ListSortDirection.Ascending));
                break;

            case ArchiveItemsSort.AlphabeticalDescending:
                FilesList?.Items.SortDescriptions.Add(new SortDescription(nameof(FileViewModel.FileName), ListSortDirection.Descending));
                DirTreeView?.Items.SortDescriptions.Add(new SortDescription(nameof(DirectoryViewModel.DisplayName), ListSortDirection.Descending));
                break;
        }
    }

    #endregion

    #region Event Handlers

    private async void DirTreeView_OnSelectedItemChangedAsync(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is DirectoryViewModel newValue)
            await ViewModel.ChangeLoadedDirAsync(e.OldValue as DirectoryViewModel, newValue);
    }

    private void DirTreeView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Select the tree view item on right click
        (e.OriginalSource as DependencyObject)?.FindParent<TreeViewItem>()?.Focus();

        e.Handled = true;
    }

    private void DirTreeItem_OnSelected(object sender, RoutedEventArgs e)
    {
        (e.OriginalSource as TreeViewItem)?.BringIntoView();
    }

    private void ArchiveExplorer_Loaded(object sender, RoutedEventArgs e)
    {
        // Make sure this won't get called again
        Loaded -= ArchiveExplorer_Loaded;

        // Refresh the sort
        RefreshSort();

        // Disable the closing button when loading
        ViewModel.LoaderViewModel.IsRunningChanged += (_, _) =>
            WindowInstance.CanClose = !ViewModel.LoaderViewModel.IsRunning;
    }

    private void FileItem_ToolTipOpening(object sender, ToolTipEventArgs e)
    {
        // Update the tool tip info each time it's shown
        sender.CastTo<ListBoxItem>().GetBindingExpression(ToolTipProperty)?.UpdateTarget();
    }

    private void FileItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Only allow double-clicking from the left mouse button
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        // Double-clicking calls the first view/edit action (opening the file in its native format)
        if (sender is FrameworkElement f && f.DataContext is FileViewModel file)
            file.EditActions.FirstOrDefault()?.MenuCommand.Execute(null);
    }

    private void FileItem_Selected(object sender, RoutedEventArgs e)
    {
        (e.OriginalSource as ListBoxItem)?.BringIntoView();
    }

    private void SortMenuItem_OnChecked(object sender, RoutedEventArgs e) => RefreshSort();

    private async void FilesList_OnDrop(object sender, DragEventArgs e)
    {
        // Make sure there is file drop data
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        // Get the files
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return;

        // Get the currently selected directory
        DirectoryViewModel? dir = ViewModel.SelectedDir;

        if (dir == null)
            return;

        // Run as a load operation
        using (LoadState state = await dir.Archive.LoaderViewModel.RunAsync(Metro.Resources.Archive_AddFiles_Status, canCancel: true))
        {
            // Lock the access to the archive
            using (await dir.Archive.ArchiveLock.LockAsync())
            {
                try
                {
                    // Add the files
                    await dir.AddFilesAsync(
                        files: files.Select(x => new FileSystemPath(x)).Where(x => x.FileExists), 
                        progressCallback: x => state.SetProgress(x), 
                        cancellationToken: state.CancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Trace(ex, "Cancelled adding files to archive");
                }

            }
        }
    }

    private void DirTreeItem_OnExpanded(object sender, RoutedEventArgs e)
    {
        if (sender is TreeViewItem t && t.DataContext is DirectoryViewModel d && d.Parent != null && !d.Any())
            t.IsExpanded = false;
    }

    private void ForceRepackMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var a in ViewModel.Archives)
            a.AddModifiedFiles(0, true);
    }

    private void FilesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var count = 0;

        foreach (FileViewModel file in sender.CastTo<ListBox>().SelectedItems)
        {
            // If a file is selected which has not been initialized we deselect it and return (since this event will be triggered again then)
            if (!file.IsInitialized)
            {
                file.IsSelected = false;
                return;
            }

            count++;
        }

        // Check if multiple files are selected
        ViewModel.AreMultipleFilesSelected = count > 1;

        ViewModel.RefreshStatusBar();
    }

    private void FileItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // Don't allow the context menu to open if not initialized
        if (((sender as FrameworkElement)?.DataContext as FileViewModel)?.IsInitialized != true)
            e.Handled = true;
    }

    private void FilesList_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
        if (r.VisualHit.GetType() != typeof(ListBoxItem))
            (sender as ListBox)?.UnselectAll();
    }

    #endregion
}
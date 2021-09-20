using RayCarrot.IO;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for ArchiveExplorer.xaml
    /// </summary>
    public partial class ArchiveExplorerUI : WindowContentControl
    {
        #region Constructor
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vm">The view model</param>
        public ArchiveExplorerUI(ArchiveExplorerDialogViewModel vm)
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
            WindowInstance.Width = 900;
            WindowInstance.Height = 600;
        }

        protected override async Task<bool> ClosingAsync()
        {
            if (!await base.ClosingAsync())
                return false;

            // Cancel the closing if an archive is running an operation
            return !ViewModel.IsLoading;
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
                case UserData_Archive_Sort.AlphabeticalAscending:
                    FilesList?.Items.SortDescriptions.Add(new SortDescription(nameof(ArchiveFileViewModel.FileName), ListSortDirection.Ascending));
                    DirTreeView?.Items.SortDescriptions.Add(new SortDescription(nameof(ArchiveDirectoryViewModel.DisplayName), ListSortDirection.Ascending));
                    break;

                case UserData_Archive_Sort.AlphabeticalDescending:
                    FilesList?.Items.SortDescriptions.Add(new SortDescription(nameof(ArchiveFileViewModel.FileName), ListSortDirection.Descending));
                    DirTreeView?.Items.SortDescriptions.Add(new SortDescription(nameof(ArchiveDirectoryViewModel.DisplayName), ListSortDirection.Descending));
                    break;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            DataContext = null;
            ViewModel?.Dispose();
        }

        #endregion

        #region Event Handlers

        private async void DirTreeView_OnSelectedItemChangedAsync(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ArchiveDirectoryViewModel newValue)
                await ViewModel.ChangeLoadedDirAsync(e.OldValue as ArchiveDirectoryViewModel, newValue);
        }

        private void DirTreeView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Select the tree view item on right click
            (e.OriginalSource as DependencyObject)?.FindParent<TreeViewItem>()?.Focus();

            e.Handled = true;
        }

        private void ArchiveExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            // Make sure this won't get called again
            Loaded -= ArchiveExplorer_Loaded;

            // Refresh the sort
            RefreshSort();

            // Create events
            ViewModel.PropertyChanged += (_, ee) =>
            {
                // Disable the closing button when loading
                if (ee.PropertyName == nameof(ArchiveExplorerDialogViewModel.IsLoading))
                    WindowInstance.CanClose = !ViewModel.IsLoading;
            };
        }

        private void FileItem_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            // Update the tool tip info each time it's shown
            sender.CastTo<ListBoxItem>().GetBindingExpression(ToolTipProperty)?.UpdateTarget();
        }

        private void ToolTipOpening_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Only allow double-clicking from the left mouse button
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            // Double-clicking calls the first view/edit action (opening the file in its native format)
            if (sender is FrameworkElement f && f.DataContext is ArchiveFileViewModel file)
                file.EditActions.FirstOrDefault()?.MenuCommand.Execute(null);
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
            var dir = ViewModel.SelectedDir;

            if (dir == null)
                return;

            // Run as a load operation
            using (dir.Archive.LoadOperation.Run())
            {
                // Lock the access to the archive
                using (await dir.Archive.ArchiveLock.LockAsync())
                {
                    // Add the files
                    await dir.AddFilesAsync(files.Select(x => new FileSystemPath(x)).Where(x => x.FileExists));
                }
            }
        }

        private void DirTreeItem_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem t && t.DataContext is ArchiveDirectoryViewModel d && d.Parent != null && !d.Any())
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

            foreach (ArchiveFileViewModel file in sender.CastTo<ListBox>().SelectedItems)
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

        private void FileContextMenu_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Don't allow the context menu to open if not initialized
            if (((sender as FrameworkElement)?.DataContext as ArchiveFileViewModel)?.IsInitialized != true)
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
}
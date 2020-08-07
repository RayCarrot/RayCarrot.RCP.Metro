using MahApps.Metro.Controls;
using RayCarrot.Common;
using RayCarrot.WPF;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for ArchiveExplorer.xaml
    /// </summary>
    public partial class ArchiveExplorerUI : UserControl, IWindowBaseControl<ArchiveExplorerDialogViewModel>
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

        /// <summary>
        /// The dialog content
        /// </summary>
        public object UIContent => this;

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        public bool Resizable => true;

        /// <summary>
        /// The base size for the dialog
        /// </summary>
        public DialogBaseSize BaseSize => DialogBaseSize.Largest;

        /// <summary>
        /// The window the control belongs to
        /// </summary>
        public MetroWindow ParentWindow { get; set; }

        #endregion

        #region Public Methods

        public void RefreshSort()
        {
            // Clear existing sort descriptions
            FilesList?.Items.SortDescriptions.Clear();
            DirTreeView?.Items.SortDescriptions.Clear();

            switch (RCPServices.Data.ArchiveExplorerSortOption)
            {
                case ArchiveExplorerSort.AlphabeticalAscending:
                    FilesList?.Items.SortDescriptions.Add(new SortDescription(nameof(ArchiveFileViewModel.FileName), ListSortDirection.Ascending));
                    DirTreeView?.Items.SortDescriptions.Add(new SortDescription(nameof(ArchiveDirectoryViewModel.DisplayName), ListSortDirection.Ascending));
                    break;

                case ArchiveExplorerSort.AlphabeticalDescending:
                    FilesList?.Items.SortDescriptions.Add(new SortDescription(nameof(ArchiveFileViewModel.FileName), ListSortDirection.Descending));
                    DirTreeView?.Items.SortDescriptions.Add(new SortDescription(nameof(ArchiveDirectoryViewModel.DisplayName), ListSortDirection.Descending));
                    break;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
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

            // Get the parent window
            ParentWindow = Window.GetWindow(this).CastTo<MetroWindow>();

            // Create events
            ParentWindow.Closing += (ss, ee) =>
            {
                // Cancel the closing if an archive is running an operation (the window can otherwise be closed with F4 etc.)
                if (ViewModel.IsLoading)
                    ee.Cancel = true;
            };

            ParentWindow.Closed += (ss, ee) =>
            {
                DataContext = null;
                ViewModel?.Dispose();
            };

            ViewModel.PropertyChanged += (ss, ee) =>
            {
                // Disable the closing button when loading
                if (ee.PropertyName == nameof(ArchiveExplorerDialogViewModel.IsLoading))
                    ParentWindow.IsCloseButtonEnabled = !ViewModel.IsLoading;
            };
        }

        private void EventSetter_OnHandler(object sender, ToolTipEventArgs e)
        {
            // Update the tool tip info each time it's shown
            sender.CastTo<ListBoxItem>().GetBindingExpression(ToolTipProperty)?.UpdateTarget();
        }

        private void SortMenuItem_OnChecked(object sender, RoutedEventArgs e) => RefreshSort();

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

        #endregion
    }
}
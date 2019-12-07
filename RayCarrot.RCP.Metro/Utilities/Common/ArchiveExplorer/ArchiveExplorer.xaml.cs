using RayCarrot.WPF;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Nito.AsyncEx;
using RayCarrot.Extensions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for ArchiveExplorer.xaml
    /// </summary>
    public partial class ArchiveExplorer : UserControl, IWindowBaseControl<ArchiveExplorerDialogViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vm">The view model</param>
        public ArchiveExplorer(ArchiveExplorerDialogViewModel vm)
        {
            InitializeComponent();
            ViewModel = vm;
            DataContext = ViewModel;
            AsyncLock = new AsyncLock();
        }

        #endregion

        #region Private Properties

        private bool IsRefreshingImages { get; set; }

        private bool CancelRefreshingImages { get; set; }

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
        /// The async lock to use for refreshing the images
        /// </summary>
        public AsyncLock AsyncLock { get; }

        #endregion

        #region Public Methods

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
            if (IsRefreshingImages)
                CancelRefreshingImages = true;

            // Lock to avoid it running multiple times at once
            using (await AsyncLock.LockAsync())
            {
                try
                {
                    IsRefreshingImages = true;

                    // Get the items
                    var prevDir = e.OldValue?.CastTo<ArchiveDirectoryViewModel>();
                    var newDir = e.NewValue?.CastTo<ArchiveDirectoryViewModel>();

                    // Remove all thumbnail image sources from memory
                    prevDir?.Files.ForEach(x => x.ThumbnailSource = null);

                    // Make sure we have a new directory
                    if (newDir?.Files == null)
                        return;

                    // Load the thumbnail image sources for the new directory
                    await Task.Run(() =>
                    {
                        // Load the thumbnail for each file
                        foreach (var x in newDir.Files)
                        {
                            x.LoadThumbnail();

                            // Check if the operation should be canceled
                            if (CancelRefreshingImages)
                                return;
                        }
                    });
                }
                finally
                {
                    IsRefreshingImages = false;
                    CancelRefreshingImages = false;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

        #endregion
    }
}
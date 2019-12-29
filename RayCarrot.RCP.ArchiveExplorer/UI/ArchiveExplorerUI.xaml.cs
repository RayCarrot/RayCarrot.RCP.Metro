using MahApps.Metro.Controls;
using RayCarrot.Extensions;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.UI;

namespace RayCarrot.RCP.ArchiveExplorer
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ViewModel?.Dispose();
        }

        #endregion

        #region Public Static Methods

        // IDEA: Move to UI manager
        /// <summary>
        /// Shows a new instance of the archive explorer, catching any potential exceptions
        /// </summary>
        /// <param name="manager">The archive data manager</param>
        /// <param name="filePaths">The archive file paths</param>
        /// <returns>The task</returns>
        public static async Task ShowAsync(IArchiveDataManager manager, IEnumerable<FileSystemPath> filePaths)
        {
            try
            {
                if (Application.Current.Dispatcher == null)
                    throw new Exception("The application does not have a valid dispatcher");

                // Run on UI thread
                await Application.Current.Dispatcher.Invoke(() => new ArchiveExplorerUI(new ArchiveExplorerDialogViewModel(manager, filePaths))).ShowWindowAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Archive explorer");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, ArchiveExplorer.Resources.Archive_CriticalError);
            }
        }


        #endregion

        #region Event Handlers

        private void ArchiveExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            // Make sure this won't get called again
            Loaded -= ArchiveExplorer_Loaded;

            // Get the parent window
            ParentWindow = Window.GetWindow(this).CastTo<MetroWindow>();

            // Create events
            ParentWindow.Closing += (ss, ee) =>
            {
                // Cancel the closing if an archive is running an operation (the window can otherwise be closed with F4 etc.)
                if (ViewModel.IsLoading)
                    ee.Cancel = true;
            };

            ParentWindow.Closed += (ss, ee) => ViewModel?.Dispose();

            ViewModel.PropertyChanged += (ss, ee) =>
            {
                if (ee.PropertyName == nameof(ArchiveExplorerDialogViewModel.IsLoading))
                    ParentWindow.IsCloseButtonEnabled = !ViewModel.IsLoading;
            };
        }

        private void EventSetter_OnHandler(object sender, ToolTipEventArgs e)
        {
            // Update the tool tip info each time it's shown
            sender.CastTo<ListBoxItem>().GetBindingExpression(ToolTipProperty)?.UpdateTarget();
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
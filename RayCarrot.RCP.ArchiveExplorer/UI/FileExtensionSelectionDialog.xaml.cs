using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RayCarrot.UI;
using RayCarrot.WPF;

namespace RayCarrot.RCP.ArchiveExplorer
{
    /// <summary>
    /// Interaction logic for FileExtensionSelectionDialog.xaml
    /// </summary>
    public partial class FileExtensionSelectionDialog : UserControl, IDialogBaseControl<FileExtensionSelectionDialogViewModel, FileExtensionSelectionDialogResult>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vm">The view model</param>
        public FileExtensionSelectionDialog(FileExtensionSelectionDialogViewModel vm)
        {
            InitializeComponent();
            ViewModel = vm;
            DataContext = ViewModel;
            CanceledByUser = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the dialog was canceled by the user, default is true
        /// </summary>
        public bool CanceledByUser { get; set; }

        /// <summary>
        /// The view model
        /// </summary>
        public FileExtensionSelectionDialogViewModel ViewModel { get; }

        /// <summary>
        /// The dialog content
        /// </summary>
        public object UIContent => this;

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        public bool Resizable => false;

        /// <summary>
        /// The base size for the dialog
        /// </summary>
        public DialogBaseSize BaseSize => DialogBaseSize.Small;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        public FileExtensionSelectionDialogResult GetResult()
        {
            return new FileExtensionSelectionDialogResult()
            {
                CanceledByUser = CanceledByUser,
                SelectedFileFormat = ViewModel.SelectedFileFormat
            };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

        #endregion

        #region Event Handlers

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CanceledByUser = false;

            // Close the dialog
            CloseDialog?.Invoke(this, new EventArgs());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the dialog
            CloseDialog?.Invoke(this, new EventArgs());
        }

        #endregion
    }

    /// <summary>
    /// View model for a file extension selection dialog
    /// </summary>
    public class FileExtensionSelectionDialogViewModel : UserInputViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileFormats">The available file formats</param>
        /// <param name="header">The header to display</param>
        public FileExtensionSelectionDialogViewModel(string[] fileFormats, string header)
        {
            // Set properties
            FileFormats = fileFormats;
            Header = header;
            SelectedFileFormat = FileFormats.First();

            // Set the default title
            Title = Resources.Archive_FileExtensionSelectionHeader;
        }

        /// <summary>
        /// The header to display
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// The available file formats
        /// </summary>
        public string[] FileFormats { get; }

        /// <summary>
        /// The selected file format
        /// </summary>
        public string SelectedFileFormat { get; set; }
    }

    /// <summary>
    /// The result for a file extension selection dialog
    /// </summary>
    public class FileExtensionSelectionDialogResult : UserInputResult
    {
        /// <summary>
        /// The selected file format
        /// </summary>
        public string SelectedFileFormat { get; set; }
    }
}
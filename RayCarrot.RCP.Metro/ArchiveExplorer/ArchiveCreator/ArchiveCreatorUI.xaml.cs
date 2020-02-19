using MahApps.Metro.Controls;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for ArchiveCreatorUI.xaml
    /// </summary>
    public partial class ArchiveCreatorUI : UserControl, IWindowBaseControl<ArchiveCreatorDialogViewModel>
    {
        #region Constructor
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vm">The view model</param>
        public ArchiveCreatorUI(ArchiveCreatorDialogViewModel vm)
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
        public ArchiveCreatorDialogViewModel ViewModel { get; }

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
        { }

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

            ViewModel.PropertyChanged += (ss, ee) =>
            {
                // Disable the closing button when loading
                if (ee.PropertyName == nameof(ArchiveExplorerDialogViewModel.IsLoading))
                    ParentWindow.IsCloseButtonEnabled = !ViewModel.IsLoading;
            };
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

        #endregion
    }

    /// <summary>
    /// View model for an archive creator dialog
    /// </summary>
    public class ArchiveCreatorDialogViewModel : UserInputViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="manager">The manager</param>
        public ArchiveCreatorDialogViewModel(IArchiveDataManager manager)
        {
            // Set properties
            Manager = manager;

            // Create commands
            CreateArchiveCommand = new AsyncRelayCommand(CreateArchiveAsync);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The manager
        /// </summary>
        public IArchiveDataManager Manager { get; }

        /// <summary>
        /// Indicates if the creator tool is loading
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// The selected input directory
        /// </summary>
        public FileSystemPath InputDirectory { get; set; }

        /// <summary>
        /// The selected output file
        /// </summary>
        public FileSystemPath OutputFile { get; set; }

        #endregion

        #region Commands

        public ICommand CreateArchiveCommand { get; }

        #endregion

        /// <summary>
        /// Creates an archive
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateArchiveAsync()
        {
            try
            {
                if (IsLoading)
                    return;

                IsLoading = true;

                // Make sure the input directory exists
                if (!InputDirectory.DirectoryExists)
                {
                    // TODO-UPDATE: Message

                    return;
                }

                // Get the import data for each file
                var importData = new List<ArchiveImportData>();

                // Add every file
                foreach (var file in Directory.GetFiles(InputDirectory, "*", SearchOption.AllDirectories))
                {
                    // Add the import data
                    importData.Add(new ArchiveImportData(Manager.GetFileEntry(file - InputDirectory),
                        // TODO-UPDATE: Encode the data
                        y => File.ReadAllBytes(file)));
                }

                // Get a new archive
                var archive = Manager.GetArchive(importData);

                // Open the output file
                using var outputStream = File.Open(OutputFile, FileMode.Create, FileAccess.Write);

                Manager.UpdateArchive(archive, outputStream, importData, null);

                // TODO-UPDATE: Success message
            }
            catch (Exception ex)
            {
                // TODO-UPDATE: Handle crash
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
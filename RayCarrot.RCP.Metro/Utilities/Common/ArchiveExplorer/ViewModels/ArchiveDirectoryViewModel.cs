using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a directory in an archive
    /// </summary>
    public class ArchiveDirectoryViewModel : HierarchicalViewModel<ArchiveDirectoryViewModel>
    {
        #region Constructors

        /// <summary>
        /// Creates a directory item view model with a directory name
        /// </summary>
        /// <param name="dirName">The directory name</param>
        protected ArchiveDirectoryViewModel(string dirName) : base(dirName)
        {
            // Create the file collection
            Files = new ObservableCollection<ArchiveFileViewModel>();

            // Create commands
            ExportCommand = new AsyncRelayCommand(ExportAsync);
            ImportCommand = new AsyncRelayCommand(ImportAsync);
        }

        /// <summary>
        /// Creates a directory item view model with the parent and directory name
        /// </summary>
        /// <param name="parent">The parent directory</param>
        /// <param name="dirName">The directory name</param>
        protected ArchiveDirectoryViewModel(ArchiveDirectoryViewModel parent, string dirName) : base(parent, dirName)
        {
            // Create the file collection
            Files = new ObservableCollection<ArchiveFileViewModel>();

            // Create commands
            ExportCommand = new AsyncRelayCommand(ExportAsync);
            ImportCommand = new AsyncRelayCommand(ImportAsync);
        }

        #endregion

        #region Commands

        public ICommand ExportCommand { get; }

        public ICommand ImportCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new directory item to the view model
        /// </summary>
        /// <param name="dirName">The name of the directory. This is not the full relative path.</param>
        /// <returns>The item</returns>
        public ArchiveDirectoryViewModel Add(string dirName)
        {
            // Create the item
            var item = new ArchiveDirectoryViewModel(this, dirName);

            // Add the item
            Add(item);

            // Return the item
            return item;
        }

        /// <summary>
        /// Exports the directory
        /// </summary>
        /// <returns>The task</returns>
        public async Task ExportAsync()
        {
            // Get the output path
            var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                // TODO: Localize
                Title = "Export files to..."
            });

            if (result.CanceledByUser)
                return;

            // Make sure the directory doesn't exist
            if ((result.SelectedDirectory + DisplayName).DirectoryExists)
            {
                // TODO: Error message
                return;
            }

            // TODO: Try/catch
            // TODO: Progress bar

            // Save the selected the format for each collection
            Dictionary<string, int> selectedFormats = new Dictionary<string, int>();

            // Select the format for each distinct collection
            foreach (var formatGroup in this.GetAllChildren(true).SelectMany(x => x.Files).GroupBy(x => x.FileData.FileFormatName))
            {
                var data = formatGroup.First().FileData;

                // TODO: Dialog for selecting file format for this file type

                selectedFormats.Add(data.FileFormatName, 1);
            }

            // Handle each directory
            foreach (var item in this.GetAllChildren(true))
            {
                // Get the directory path
                var path = result.SelectedDirectory + DisplayName + item.FullPath.Remove(0, FullPath.Length).Trim(Path.DirectorySeparatorChar);

                // Create the directory
                Directory.CreateDirectory(path);

                // Save each file
                foreach (var file in item.Files)
                {
                    // Get the selected format
                    var format = file.FileData.AvailableFileFormats[selectedFormats[file.FileData.FileFormatName]].Filter;

                    // Save the file
                    await file.FileData.ExportFileAsync(file.ArchiveFileStream, 
                        path + (new FileSystemPath(file.FileName).ChangeFileExtension(format)), format);
                }
            }

            // TODO: Success message
        }

        /// <summary>
        /// Imports files to the directory
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportAsync()
        {
            // TODO: Finish
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The files
        /// </summary>
        public ObservableCollection<ArchiveFileViewModel> Files { get; }

        /// <summary>
        /// Indicates if the item is selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Indicates if the item is expanded
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// The name of the item to display
        /// </summary>
        public virtual string DisplayName => ID;

        /// <summary>
        /// The full directory path
        /// </summary>
        public string FullPath => FullID.JoinItems(Path.DirectorySeparatorChar.ToString());

        #endregion
    }
}
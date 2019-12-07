using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for an archive explorer dialog
    /// </summary>
    public class ArchiveExplorerDialogViewModel : UserInputViewModel, IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="filePath">The archive file path</param>
        public ArchiveExplorerDialogViewModel(FileSystemPath filePath)
        {
            // Create properties
            Directories = new ArchiveDirectoryViewModel();

            // Create the file stream
            ArchiveFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Read the CNT data
            Data = new R2CntSerializer().Deserialize(ArchiveFileStream);

            // Add the directories to the collection
            for (var i = 0; i < Data.Directories.Length; i++)
            {
                // Keep track of the previous item
                var prevItem = Directories;

                // Enumerate each sub directory
                foreach (string dir in Data.Directories[i].Split('\\'))
                {
                    // Get the directory name
                    var dirName = Path.GetFileName(dir);

                    // Set the previous item and create the item if it doesn't already exist
                    prevItem = prevItem.FindItem(x => x.ID == dirName) ?? prevItem.Add(dirName);
                }

                // Add the files
                foreach (var f in Data.Files.Where(x => x.DirectoryIndex == i))
                {
                    // Add the file
                    prevItem.Files.Add(new ArchiveFileViewModel(f, ArchiveFileStream));
                }
            }
        }

        /// <summary>
        /// The directories
        /// </summary>
        public ArchiveDirectoryViewModel Directories { get; }

        /// <summary>
        /// The archive data
        /// </summary>
        public R2CntData Data { get; }

        /// <summary>
        /// The archive file stream
        /// </summary>
        public FileStream ArchiveFileStream { get; }

        public void Dispose()
        {
            ArchiveFileStream?.Dispose();
        }
    }

    /// <summary>
    /// View model for a directory in an archive
    /// </summary>
    public class ArchiveDirectoryViewModel : HierarchicalViewModel<ArchiveDirectoryViewModel>
    {
        #region Constructors

        /// <summary>
        /// Creates a generic parent directory item view model which contains other items
        /// </summary>
        public ArchiveDirectoryViewModel() : base(String.Empty)
        {

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
        }

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

        #endregion

        #region Public Properties

        /// <summary>
        /// The files
        /// </summary>
        public ObservableCollection<ArchiveFileViewModel> Files { get; }

        #endregion
    }

    /// <summary>
    /// View model for a file in an archive
    /// </summary>
    public class ArchiveFileViewModel : BaseRCPViewModel
    {
        #region Constructor

        public ArchiveFileViewModel(R2CntFile fileData, FileStream cntFileStream)
        {
            FileData = fileData;
            CNTFileStream = cntFileStream;
            FileName = FileData.FileName;
        }

        #endregion

        #region Public Properties

        public R2CntFile FileData { get; }

        public ImageSource ThumbnailSource { get; set; }

        protected FileStream CNTFileStream { get; }

        public string FileName { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the thumbnail image source for the file
        /// </summary>
        public void LoadThumbnail()
        {
            // Get the thumbnail
            var img = FileData.
                // Get the image file content from the stream
                GetFileContent(CNTFileStream).
                // Get the bitmap image
                GetBitmapThumbnail(64, 64).
                // Get an image source from the bitmap
                ToImageSource();

            // Freeze the image to avoid thread errors
            img.Freeze();

            // Set the image source
            ThumbnailSource = img;
        }

        #endregion
    }
}
using System.Collections.ObjectModel;
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

        #endregion
    }
}
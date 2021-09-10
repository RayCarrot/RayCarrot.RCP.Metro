using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A browse UI Manager for managing browsing UI requests
    /// </summary>
    public interface IBrowseUIManager
    {
        /// <summary>
        /// Allows the user to browse for a file
        /// </summary>
        /// <param name="fileBrowserModel">The file browser information</param>
        /// <returns>The file browser result</returns>
        Task<FileBrowserResult> BrowseFileAsync(FileBrowserViewModel fileBrowserModel);

        /// <summary>
        /// Allows the user to browse for a directory
        /// </summary>
        /// <param name="directoryBrowserModel">The directory browser information</param>
        /// <returns>The directory browser result</returns>
        Task<DirectoryBrowserResult> BrowseDirectoryAsync(DirectoryBrowserViewModel directoryBrowserModel);

        /// <summary>
        /// Allows the user to browse for a location
        /// and chose a name for a file to save
        /// </summary>
        /// <param name="saveFileModel">The save file browser information</param>
        /// <returns>The save file result</returns>
        Task<SaveFileResult> SaveFileAsync(SaveFileViewModel saveFileModel);

        /// <summary>
        /// Allows the user to browse for a drive
        /// </summary>
        /// <param name="driveBrowserModel">The drive browser information</param>
        /// <returns>The browse drive result</returns>
        Task<DriveBrowserResult> BrowseDriveAsync(DriveBrowserViewModel driveBrowserModel);
    }
}
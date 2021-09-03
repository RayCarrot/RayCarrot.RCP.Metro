using System.Runtime.CompilerServices;
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
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The file browser result</returns>
        Task<FileBrowserResult> BrowseFileAsync(FileBrowserViewModel fileBrowserModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0);

        /// <summary>
        /// Allows the user to browse for a directory
        /// </summary>
        /// <param name="directoryBrowserModel">The directory browser information</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The directory browser result</returns>
        Task<DirectoryBrowserResult> BrowseDirectoryAsync(DirectoryBrowserViewModel directoryBrowserModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0);

        /// <summary>
        /// Allows the user to browse for a location
        /// and chose a name for a file to save
        /// </summary>
        /// <param name="saveFileModel">The save file browser information</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The save file result</returns>
        Task<SaveFileResult> SaveFileAsync(SaveFileViewModel saveFileModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0);

        /// <summary>
        /// Allows the user to browse for a drive
        /// </summary>
        /// <param name="driveBrowserModel">The drive browser information</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The browse drive result</returns>
        Task<DriveBrowserResult> BrowseDriveAsync(DriveBrowserViewModel driveBrowserModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0);
    }
}
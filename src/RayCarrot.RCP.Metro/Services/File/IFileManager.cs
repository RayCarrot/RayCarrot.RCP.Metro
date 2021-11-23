using System.Diagnostics;
using System.Threading.Tasks;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;
// TODO: Clean up and split up - move some of these to FileSystemPathExtensions

/// <summary>
/// Defines a generic file manager
/// </summary>
public interface IFileManager
{
    /// <summary>
    /// Launches a file
    /// </summary>
    /// <param name="file">The file to launch</param>
    /// <param name="asAdmin">True if it should be run as admin, otherwise false</param>
    /// <param name="arguments">The launch arguments</param>
    /// <param name="wd">The working directory, or null if the parent directory</param>
    Task<Process> LaunchFileAsync(FileSystemPath file, bool asAdmin = false, string arguments = null, string wd = null);

    /// <summary>
    /// Creates a file shortcut
    /// </summary>
    void CreateFileShortcut(FileSystemPath ShortcutName, FileSystemPath DestinationDirectory, FileSystemPath TargetFile, string arguments = null);

    /// <summary>
    /// Creates a shortcut for an URL
    /// </summary>
    /// <param name="shortcutName">The name of the shortcut file</param>
    /// <param name="destinationDirectory">The path of the directory</param>
    /// <param name="targetURL">The URL</param>
    void CreateURLShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory, string targetURL);

    /// <summary>
    /// Opens the specified path in Explorer
    /// </summary>
    /// <param name="location">The path</param>
    Task OpenExplorerLocationAsync(FileSystemPath location);

    /// <summary>
    /// Opens the specified registry key path in RegEdit
    /// </summary>
    /// <param name="registryKeyPath">The key path to open</param>
    /// <returns>The task</returns>
    Task OpenRegistryKeyAsync(string registryKeyPath);

    /// <summary>
    /// Deletes a directory recursively if it exists
    /// </summary>
    /// <param name="dirPath">The directory path</param>
    void DeleteDirectory(FileSystemPath dirPath);

    /// <summary>
    /// Creates an new empty file
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="overwrite">Indicates if an existing file with the same name should be overwritten or else ignored</param>
    void CreateFile(FileSystemPath filePath, bool overwrite = true);

    /// <summary>
    /// Deletes a file if it exists
    /// </summary>
    /// <param name="filePath">The file path</param>
    void DeleteFile(FileSystemPath filePath);

    /// <summary>
    /// Moves a directory and creates the parent directory of its new location if it doesn't exist
    /// </summary>
    /// <param name="source">The source directory path</param>
    /// <param name="destination">The destination directory path</param>
    /// <param name="replaceDir">Indicates if the destination should be replaced if it already exists</param>
    /// <param name="replaceExistingFiles">Indicates if any existing files with the same name should be replaced</param>
    void MoveDirectory(FileSystemPath source, FileSystemPath destination, bool replaceDir, bool replaceExistingFiles);

    /// <summary>
    /// Moves files from a specified source to a new destination
    /// </summary>
    /// <param name="source">The source directory and search pattern to use when finding files</param>
    /// <param name="destination">The destination directory path</param>
    /// <param name="replaceExistingFiles">Indicates if any existing files with the same name should be replaced</param>
    void MoveFiles(IOSearchPattern source, FileSystemPath destination, bool replaceExistingFiles);

    /// <summary>
    /// Copies a directory and creates the parent directory of its new location if it doesn't exist
    /// </summary>
    /// <param name="source">The source directory path</param>
    /// <param name="destination">The destination directory path</param>
    /// <param name="replaceDir">Indicates if the destination should be replaced if it already exists</param>
    /// <param name="replaceExistingFiles">Indicates if any existing files with the same name should be replaced</param>
    void CopyDirectory(FileSystemPath source, FileSystemPath destination, bool replaceDir, bool replaceExistingFiles);

    /// <summary>
    /// Copies files from a specified source to a new destination
    /// </summary>
    /// <param name="source">The source directory and search pattern to use when finding files</param>
    /// <param name="destination">The destination directory path</param>
    /// <param name="replaceExistingFiles">Indicates if any existing files with the same name should be replaced</param>
    void CopyFiles(IOSearchPattern source, FileSystemPath destination, bool replaceExistingFiles);

    /// <summary>
    /// Moves a file and creates the parent directory of its new location if it doesn't exist
    /// </summary>
    /// <param name="source">The source file path</param>
    /// <param name="destination">The destination file path</param>
    /// <param name="replace">Indicates if the destination should be replaced if it already exists</param>
    void MoveFile(FileSystemPath source, FileSystemPath destination, bool replace);

    /// <summary>
    /// Copies a file and creates the parent directory of its new location if it doesn't exist
    /// </summary>
    /// <param name="source">The source file path</param>
    /// <param name="destination">The destination file path</param>
    /// <param name="replace">Indicates if the destination should be replaced if it already exists</param>
    void CopyFile(FileSystemPath source, FileSystemPath destination, bool replace);

    /// <summary>
    /// Checks if the specified file has write access
    /// </summary>
    /// <param name="path">The file to check</param>
    /// <returns>True if the file can be written to, otherwise false</returns>
    bool CheckFileWriteAccess(FileSystemPath path);

    /// <summary>
    /// Checks if the specified directory has write access
    /// </summary>
    /// <param name="path">The directory to check</param>
    /// <returns>True if the directory can be written to, otherwise false</returns>
    bool CheckDirectoryWriteAccess(FileSystemPath path);
}
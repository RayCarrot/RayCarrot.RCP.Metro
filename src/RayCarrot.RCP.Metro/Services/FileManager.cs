#nullable disable
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The default file manager for the Rayman Control Panel
/// </summary>
public class FileManager
{
    public FileManager(IMessageUIManager message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private IMessageUIManager Message { get; }

    /// <summary>
    /// Launches a file
    /// </summary>
    /// <param name="file">The file to launch</param>
    /// <param name="asAdmin">True if it should be run as admin, otherwise false</param>
    /// <param name="arguments">The launch arguments</param>
    /// <param name="wd">The working directory, or null if the parent directory</param>
    public async Task<Process> LaunchFileAsync(FileSystemPath file, bool asAdmin = false, string arguments = null, string wd = null)
    {
        try
        {
            // Create the process start info
            ProcessStartInfo info = new ProcessStartInfo
            {
                // Set the file path
                FileName = file,

                // Set to working directory to the parent directory if not otherwise specified
                WorkingDirectory = wd ?? file.Parent
            };

            // Set arguments if specified
            if (arguments != null)
                info.Arguments = arguments;

            // Set to run as admin if specified
            if (asAdmin)
                info.Verb = "runas";

            // Start the process and get the process
            var p = Process.Start(info);

            Logger.Info("The file {0} launched with the arguments: {1}", file.FullPath, arguments);

            // Return the process
            return p;
        }
        catch (FileNotFoundException ex)
        {
            Logger.Debug(ex, "Launching file {0}", file);
                
            await Message.DisplayMessageAsync(String.Format(Resources.File_FileNotFound, file.FullPath), Resources.File_FileNotFoundHeader, MessageType.Error);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Launching file {0}", file);

            await Message.DisplayExceptionMessageAsync(ex, String.Format(Resources.File_ErrorLaunchingFile, file.FullPath));
        }

        // Return null if the process could not launch
        return null;
    }

    public Task LaunchURIAsync(string uri)
    {
        // NOTE: We could use Launcher.LaunchURI here, but since we're targeting Windows 7 it is good to use as few of the WinRT APIs as possible to avoid any runtime errors. Launching a file as a process will work with URLs as well, although less information will be given in case of error (such as if no application is installed to handle the URI).
        return LaunchFileAsync(uri);
    }

    /// <summary>
    /// Creates a file shortcut
    /// </summary>
    public void CreateFileShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory, FileSystemPath targetFile, string arguments = null)
    {
        try
        {
            // Make sure the file extension is correct or else Windows won't treat it as a shortcut
            shortcutName = shortcutName.ChangeFileExtension(new FileExtension(".lnk"));

            // Delete if a shortcut with the same name already exists
            DeleteFile(destinationDirectory + shortcutName);

            // Create the shortcut
            WindowsHelpers.CreateFileShortcut(shortcutName, destinationDirectory, targetFile, arguments);

            Logger.Info("The shortcut {0} was created", shortcutName);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Creating shortcut {0}", destinationDirectory);

            throw;
        }
    }

    /// <summary>
    /// Creates a shortcut for an URL
    /// </summary>
    /// <param name="shortcutName">The name of the shortcut file</param>
    /// <param name="destinationDirectory">The path of the directory</param>
    /// <param name="targetURL">The URL</param>
    public void CreateURLShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory, string targetURL)
    {
        try
        {
            // Make sure the file extension is correct or else Windows won't treat it as an URL shortcut
            shortcutName = shortcutName.ChangeFileExtension(new FileExtension(".url"));

            // Delete if a shortcut with the same name already exists
            DeleteFile(destinationDirectory + shortcutName);

            // Create the shortcut
            WindowsHelpers.CreateURLShortcut(shortcutName, destinationDirectory, targetURL);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Creating URL shortcut {0}", destinationDirectory);

            throw;
        }
    }

    /// <summary>
    /// Opens the specified path in Explorer
    /// </summary>
    /// <param name="location">The path</param>
    public async Task OpenExplorerLocationAsync(FileSystemPath location)
    {
        if (!location.Exists)
        {
            await Message.DisplayMessageAsync(Resources.File_LocationNotFound, Resources.File_OpenLocationErrorHeader, MessageType.Error);
            return;
        }

        try
        {
            WindowsHelpers.OpenExplorerPath(location);
            Logger.Debug("The explorer location {0} was opened", location);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Opening explorer location {0}", location);
                
            await Message.DisplayExceptionMessageAsync(ex, Resources.File_OpenLocationError, Resources.File_OpenLocationErrorHeader);
        }
    }

    /// <summary>
    /// Opens the specified registry key path in RegEdit
    /// </summary>
    /// <param name="registryKeyPath">The key path to open</param>
    /// <returns>The task</returns>
    public async Task OpenRegistryKeyAsync(string registryKeyPath)
    {
        if (!RegistryHelpers.KeyExists(registryKeyPath))
        {
            await Message.DisplayMessageAsync(Resources.File_RegKeyNotFound, Resources.File_RegKeyNotFoundHeader, MessageType.Error);

            return;
        }

        try
        {
            WindowsHelpers.OpenRegistryPath(registryKeyPath);
            Logger.Debug("The Registry key path {0} was opened", registryKeyPath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Opening Registry key path {0}", registryKeyPath);

            await Message.DisplayExceptionMessageAsync(ex, Resources.File_OpenRegKeyError, Resources.File_OpenRegKeyErrorHeader);
        }
    }

    /// <summary>
    /// Deletes a directory recursively if it exists
    /// </summary>
    /// <param name="dirPath">The directory path</param>
    public void DeleteDirectory(FileSystemPath dirPath)
    {
        dirPath.DeleteDirectory();
        Logger.Debug("The directory {0} was deleted", dirPath);
    }

    /// <summary>
    /// Creates an new empty file
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="overwrite">Indicates if an existing file with the same name should be overwritten or else ignored</param>
    public void CreateFile(FileSystemPath filePath, bool overwrite = true)
    {
        // Check if the file exists
        if (filePath.FileExists && !overwrite)
            return;

        // Create the parent directory
        Directory.CreateDirectory(filePath.Parent);

        // Create the file
        File.Create(filePath).Dispose();

        Logger.Debug("The file {0} was created", filePath);
    }

    /// <summary>
    /// Deletes a file if it exists
    /// </summary>
    /// <param name="filePath">The file path</param>
    public void DeleteFile(FileSystemPath filePath)
    {
        filePath.DeleteFile();

        Logger.Debug("The file {0} was deleted", filePath);
    }

    /// <summary>
    /// Moves a directory and creates the parent directory of its new location if it doesn't exist
    /// </summary>
    /// <param name="source">The source directory path</param>
    /// <param name="destination">The destination directory path</param>
    /// <param name="replaceDir">Indicates if the destination should be replaced if it already exists</param>
    /// <param name="replaceExistingFiles">Indicates if any existing files with the same name should be replaced</param>
    public void MoveDirectory(FileSystemPath source, FileSystemPath destination, bool replaceDir, bool replaceExistingFiles)
    {
        // Delete existing directory if set to replace
        if (replaceDir)
            DeleteDirectory(destination);

        // Check if the parent directory does not exist
        if (!destination.Parent.DirectoryExists)
            // Create the parent directory
            Directory.CreateDirectory(destination.Parent);

        // If we replace, the directory will be deleted, thus we can simply move it
        if (replaceDir)
        {
            // Move the directory
            Directory.Move(source, destination);
        }
        // If we do not replace we have to move file by file and directory by directory
        else
        {
            // Recreate each directory
            foreach (FileSystemPath dir in Directory.GetDirectories(source, "*", System.IO.SearchOption.AllDirectories))
            {
                // Get the destination directory
                var destDir = destination + (dir - source);

                // Create the directory
                Directory.CreateDirectory(destDir);
            }

            // Move each file
            MoveFiles(new IOSearchPattern(source), destination, replaceExistingFiles);

            // Delete the source as there will be empty directories left there now
            DeleteDirectory(source);
        }


        Logger.Debug("The directory {0} was moved to {1}", source, destination);
    }

    /// <summary>
    /// Moves files from a specified source to a new destination
    /// </summary>
    /// <param name="source">The source directory and search pattern to use when finding files</param>
    /// <param name="destination">The destination directory path</param>
    /// <param name="replaceExistingFiles">Indicates if any existing files with the same name should be replaced</param>
    public void MoveFiles(IOSearchPattern source, FileSystemPath destination, bool replaceExistingFiles)
    {
        // Move each file
        foreach (FileSystemPath file in Directory.GetFiles(source.DirPath, source.SearchPattern, source.SearchOption))
        {
            // Get the destination file
            var destFile = destination + (file - source.DirPath);

            // Skip if the file already exists and we should not replace it
            if (destFile.FileExists && !replaceExistingFiles)
                continue;

            // Move the file
            MoveFile(file, destFile, true);
        }

        Logger.Debug("The files from {0} were moved to {1}", source.DirPath, destination);
    }

    /// <summary>
    /// Copies a directory and creates the parent directory of its new location if it doesn't exist
    /// </summary>
    /// <param name="source">The source directory path</param>
    /// <param name="destination">The destination directory path</param>
    /// <param name="replaceDir">Indicates if the destination should be replaced if it already exists</param>
    /// <param name="replaceExistingFiles">Indicates if any existing files with the same name should be replaced</param>
    public void CopyDirectory(FileSystemPath source, FileSystemPath destination, bool replaceDir, bool replaceExistingFiles)
    {
        // Delete existing directory if set to replace
        if (replaceDir)
            DeleteDirectory(destination);

        // Copy the directory
        FileSystem.CopyDirectory(source, destination, replaceExistingFiles);

        Logger.Debug("The directory {0} was copied to {1}", source, destination);
    }

    /// <summary>
    /// Copies files from a specified source to a new destination
    /// </summary>
    /// <param name="source">The source directory and search pattern to use when finding files</param>
    /// <param name="destination">The destination directory path</param>
    /// <param name="replaceExistingFiles">Indicates if any existing files with the same name should be replaced</param>
    public void CopyFiles(IOSearchPattern source, FileSystemPath destination, bool replaceExistingFiles)
    {
        // Copy each file
        foreach (FileSystemPath file in Directory.GetFiles(source.DirPath, source.SearchPattern, source.SearchOption))
        {
            // Get the destination file
            var destFile = destination + (file - source.DirPath);

            // Skip if the file already exists and we should not replace it
            if (destFile.FileExists && !replaceExistingFiles)
                continue;

            // Move the file
            CopyFile(file, destFile, true);
        }

        Logger.Debug("The files from {0} were copied to {1}", source.DirPath, destination);
    }

    /// <summary>
    /// Moves a file and creates the parent directory of its new location if it doesn't exist
    /// </summary>
    /// <param name="source">The source file path</param>
    /// <param name="destination">The destination file path</param>
    /// <param name="replace">Indicates if the destination should be replaced if it already exists</param>
    public void MoveFile(FileSystemPath source, FileSystemPath destination, bool replace)
    {
        if (!source.FileExists)
            throw new FileNotFoundException("Source file does not exist");

        // Delete existing file if set to replace
        if (replace)
            DeleteFile(destination);

        // Check if the parent directory does not exist
        if (!destination.Parent.DirectoryExists)
            // Create the parent directory
            Directory.CreateDirectory(destination.Parent);

        // Move the file
        File.Move(source, destination);

        Logger.Debug("The file {0} was moved to {1}", source, destination);
    }

    /// <summary>
    /// Copies a file and creates the parent directory of its new location if it doesn't exist
    /// </summary>
    /// <param name="source">The source file path</param>
    /// <param name="destination">The destination file path</param>
    /// <param name="replace">Indicates if the destination should be replaced if it already exists</param>
    public void CopyFile(FileSystemPath source, FileSystemPath destination, bool replace)
    {
        // Delete existing file if set to replace
        if (replace)
            DeleteFile(destination);

        // Check if the parent directory does not exist
        if (!destination.Parent.DirectoryExists)
            // Create the parent directory
            Directory.CreateDirectory(destination.Parent);

        // Move the file
        File.Copy(source, destination);

        Logger.Debug("The file {0} was copied to {1}", source, destination);
    }

    /// <summary>
    /// Checks if the specified file has write access
    /// </summary>
    /// <param name="path">The file to check</param>
    /// <returns>True if the file can be written to, otherwise false</returns>
    public bool CheckFileWriteAccess(FileSystemPath path)
    {
        if (!path.FileExists)
            return false;

        try
        {
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                return true;
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Checking for file write access");
            return false;
        }
    }

    /// <summary>
    /// Checks if the specified directory has write access
    /// </summary>
    /// <param name="path">The directory to check</param>
    /// <returns>True if the directory can be written to, otherwise false</returns>
    public bool CheckDirectoryWriteAccess(FileSystemPath path)
    {
        try
        {
            Directory.CreateDirectory(path);

            using (File.Create(path + Path.GetRandomFileName(), 1, FileOptions.DeleteOnClose))
                return true;
        }
        catch
        {
            return false;
        }
    }
}
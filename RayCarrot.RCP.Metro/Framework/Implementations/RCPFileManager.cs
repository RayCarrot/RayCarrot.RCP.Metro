using Microsoft.VisualBasic.FileIO;
using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The file manager for the Rayman Control Panel
    /// </summary>
    public class RCPFileManager
    {
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
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = file,
                    WorkingDirectory = wd ?? file.Parent
                };

                if (arguments != null)
                    info.Arguments = arguments;

                if (asAdmin)
                    info.AsAdmin();

                var p = Process.Start(info);

                RCFCore.Logger?.LogInformationSource($"The file {file.FullPath} launched with the arguments: {arguments}");

                return p;
            }
            catch (FileNotFoundException ex)
            {
                ex.HandleExpected("Launching file", file);
                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.File_FileNotFound, file.FullPath), Resources.File_FileNotFoundHeader, MessageType.Error);
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Launching file", file);
                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.File_ErrorLaunchingFile, file.FullPath), MessageType.Error);
            }

            return null;
        }

        /// <summary>
        /// Creates a file shortcut
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateFileShortcutAsync(FileSystemPath ShortcutName, FileSystemPath DestinationDirectory, FileSystemPath TargetFile, string arguments = null)
        {
            try
            {
                ShortcutName = ShortcutName.ChangeFileExtension(".lnk");
                WindowsHelpers.CreateFileShortcut(ShortcutName, DestinationDirectory, TargetFile, arguments);

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.File_ShortcutCreated);
                RCFCore.Logger?.LogInformationSource($"The shortcut {ShortcutName} was created");
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Creating shortcut", DestinationDirectory);
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.File_CreatingShortcutError, Resources.File_CreatingShortcutErrorHeader, MessageType.Error);
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
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.File_LocationNotFound, Resources.File_OpenLocationErrorHeader, MessageType.Error);
                return;
            }

            try
            {
                WindowsHelpers.OpenExplorerPath(location);
                RCFCore.Logger?.LogDebugSource($"The explorer location {location} was opened");
            }
            catch (Exception ex)
            {
                ex.HandleError("Opening explorer location", location);
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.File_OpenLocationError, Resources.File_OpenLocationErrorHeader, MessageType.Error);
            }
        }

        /// <summary>
        /// Opens the specified registry key path in RegEdit
        /// </summary>
        /// <param name="registryKeyPath">The key path to open</param>
        /// <returns>The task</returns>
        public async Task OpenRegistryKeyAsync(string registryKeyPath)
        {
            if (!RCFWinReg.RegistryManager.KeyExists(registryKeyPath))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.File_RegKeyNotFound, Resources.File_RegKeyNotFoundHeader, MessageType.Error);

                return;
            }

            try
            {
                WindowsHelpers.OpenRegistryPath(registryKeyPath);
                RCFCore.Logger?.LogDebugSource($"The Registry key path {registryKeyPath} was opened");
            }
            catch (Exception ex)
            {
                ex.HandleError("Opening Registry key path", registryKeyPath);

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.File_OpenRegKeyError, Resources.File_OpenRegKeyErrorHeader, MessageType.Error);
            }
        }

        /// <summary>
        /// Deletes a directory recursively if it exists
        /// </summary>
        /// <param name="dirPath">The directory path</param>
        public void DeleteDirectory(FileSystemPath dirPath)
        {
            // Check if the directory exists
            if (!dirPath.DirectoryExists)
                return;

            // Delete the directory
            Directory.Delete(dirPath, true);

            RCFCore.Logger?.LogDebugSource($"The directory {dirPath} was deleted");
        }

        /// <summary>
        /// Creates a new empty file
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
            File.Create(filePath);

            RCFCore.Logger?.LogDebugSource($"The file {filePath} was created");
        }

        /// <summary>
        /// Deletes a file if it exists
        /// </summary>
        /// <param name="filePath">The file path</param>
        public void DeleteFile(FileSystemPath filePath)
        {
            // Check if the file exists
            if (!filePath.FileExists)
                return;

            // Delete the file
            File.Delete(filePath);

            RCFCore.Logger?.LogDebugSource($"The file {filePath} was deleted");
        }

        /// <summary>
        /// Moves a directory and creates the parent directory of its new location if it doesn't exist
        /// </summary>
        /// <param name="source">The source directory path</param>
        /// <param name="destination">The destination directory path</param>
        /// <param name="replace">Indicates if the destination should be replaced if it already exists</param>
        public void MoveDirectory(FileSystemPath source, FileSystemPath destination, bool replace)
        {
            // Delete existing directory if set to replace
            if (replace)
                DeleteDirectory(destination);

            // Check if the parent directory does not exist
            if (!destination.Parent.DirectoryExists)
                // Create the parent directory
                Directory.CreateDirectory(destination.Parent);

            // Move the directory
            Directory.Move(source, destination);

            RCFCore.Logger?.LogDebugSource($"The directory {source} was moved to {destination}");
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

            RCFCore.Logger?.LogDebugSource($"The directory {source} was copied to {destination}");
        }

        /// <summary>
        /// Moves a file and creates the parent directory of its new location if it doesn't exist
        /// </summary>
        /// <param name="source">The source file path</param>
        /// <param name="destination">The destination file path</param>
        /// <param name="replace">Indicates if the destination should be replaced if it already exists</param>
        public void MoveFile(FileSystemPath source, FileSystemPath destination, bool replace)
        {
            // Delete existing file if set to replace
            if (replace)
                DeleteFile(destination);

            // Check if the parent directory does not exist
            if (!destination.Parent.DirectoryExists)
                // Create the parent directory
                Directory.CreateDirectory(destination.Parent);

            // Move the file
            File.Move(source, destination);

            RCFCore.Logger?.LogDebugSource($"The file {source} was moved to {destination}");
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
                ex.HandleExpected("Checking for file write access");
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
                using (FileStream fs = File.Create(path + Path.GetRandomFileName(), 1, FileOptions.DeleteOnClose))
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
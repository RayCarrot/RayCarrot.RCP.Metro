using Microsoft.VisualBasic.FileIO;
using RayCarrot.CarrotFramework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    // TODO: Make universal interface in framework
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
        public async Task LaunchFileAsync(FileSystemPath file, bool asAdmin = false, string arguments = null, string wd = null)
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

                Process.Start(info);

                RCF.Logger.LogDebugSource($"The file {file} launched");
            }
            catch (FileNotFoundException ex)
            {
                ex.HandleExpected("Launching file", file);
                await RCF.MessageUI.DisplayMessageAsync(file + " could not be found. Please check the specified path.", "The file could not be found in specified path", MessageType.Error);
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Launching file", file);
                await RCF.MessageUI.DisplayMessageAsync($"An error occurred when attempting to run {file}", "Error", MessageType.Error);
            }
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

                if ((DestinationDirectory + ShortcutName).FileExists)
                {
                    await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("Shortcut created successfully");
                    RCF.Logger.LogInformationSource($"The shortcut {ShortcutName} was created");
                }
                else
                    await RCF.MessageUI.DisplayMessageAsync("An error occurred creating the shortcut", "Error creating shortcut", MessageType.Error);
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Creating shortcut", DestinationDirectory);
                await RCF.MessageUI.DisplayMessageAsync("An error occurred creating the shortcut", "Error creating shortcut", MessageType.Error);
            }
        }

        /// <summary>
        /// Opens the specified path in Explorer
        /// </summary>
        /// <param name="location">The path</param>
        public async Task OpenExplorerLocationAsync(FileSystemPath location)
        {
            if (location.Exists)
            {
                try
                {
                    WindowsHelpers.OpenExplorerPath(location);
                    RCF.Logger.LogDebugSource($"The explorer location {location} was opened");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Opening explorer location", location);
                    await RCF.MessageUI.DisplayMessageAsync("The directory could not be opened", "Directory error",
                        MessageType.Error);
                }
            }
            else
            {
                await RCF.MessageUI.DisplayMessageAsync("Location not found", "Directory error",
                    MessageType.Error);
            }
        }

        /// <summary>
        /// Deletes a directory recursively if it exists
        /// </summary>
        /// <param name="dirPath">The directory path</param>
        public void DeleteDirectory(FileSystemPath dirPath)
        {
            // Check if the directory exists
            if (dirPath.DirectoryExists)
            {
                // Delete the directory
                Directory.Delete(dirPath, true);

                RCF.Logger.LogDebugSource($"The directory {dirPath} was deleted");
            }
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

            RCF.Logger.LogDebugSource($"The file {filePath} was created");
        }

        /// <summary>
        /// Deletes a file if it exists
        /// </summary>
        /// <param name="filePath">The file path</param>
        public void DeleteFile(FileSystemPath filePath)
        {
            // Check if the file exists
            if (filePath.FileExists)
            {
                // Delete the file
                File.Delete(filePath);

                RCF.Logger.LogDebugSource($"The file {filePath} was deleted");
            }
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

            RCF.Logger.LogDebugSource($"The directory {source} was moved to {destination}");
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

            RCF.Logger.LogDebugSource($"The directory {source} was copied to {destination}");
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

            RCF.Logger.LogDebugSource($"The file {source} was moved to {destination}");
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
    }
}
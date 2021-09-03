using System;
using Microsoft.Win32;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The default <see cref="IBrowseUIManager"/> for WPF
    /// </summary>
    public class RCPBrowseUIManager : IBrowseUIManager
    {
        /// <summary>
        /// Indicates if the browse requests should be logged
        /// </summary>
        public virtual bool LogRequests { get; set; }

        /// <summary>
        /// Allows the user to browse for a directory
        /// </summary>
        /// <param name="directoryBrowserModel">The directory browser information</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The directory browser result</returns>
        public virtual Task<DirectoryBrowserResult> BrowseDirectoryAsync(DirectoryBrowserViewModel directoryBrowserModel, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (LogRequests)
                RL.Logger?.LogTraceSource($"A browse directory dialog was opened with the title of: {directoryBrowserModel.Title}", origin: origin, filePath: filePath, lineNumber: lineNumber);

            return Application.Current.Dispatcher.Invoke(() =>
            {
                using var dialog = new CommonOpenFileDialog
                {
                    Title = directoryBrowserModel.Title,
                    AllowNonFileSystemItems = false,
                    IsFolderPicker = true,
                    Multiselect = directoryBrowserModel.MultiSelection,
                    InitialDirectory = directoryBrowserModel.DefaultDirectory,
                    DefaultFileName = directoryBrowserModel.DefaultName,
                    EnsureFileExists = true,
                    EnsurePathExists = true
                };

                // Show the dialog
                var dialogResult = dialog.ShowDialog(Application.Current.Windows.Cast<Window>().FindItem(x => x.IsActive));

                var result = dialogResult != CommonFileDialogResult.Ok ? new DirectoryBrowserResult()
                {
                    CanceledByUser = true
                } : new DirectoryBrowserResult()
                {
                    CanceledByUser = false,
                    SelectedDirectory = dialog.FileName,
                    SelectedDirectories = dialog.FileNames.Select(x => new FileSystemPath(x))
                };

                RL.Logger?.LogTraceSource(result.CanceledByUser
                    ? "The browse directory dialog was canceled by the user"
                    : $"The browse directory dialog returned the selected directory paths {result.SelectedDirectories.JoinItems(", ")}");

                return Task.FromResult(result);
            });
        }

        /// <summary>
        /// Allows the user to browse for a file
        /// </summary>
        /// <param name="fileBrowserModel">The file browser information</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The file browser result</returns>
        public virtual Task<FileBrowserResult> BrowseFileAsync(FileBrowserViewModel fileBrowserModel, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (LogRequests)
                RL.Logger?.LogTraceSource($"A browse file dialog was opened with the title of: {fileBrowserModel.Title}", origin: origin, filePath: filePath, lineNumber: lineNumber);

            return Application.Current.Dispatcher.Invoke(() =>
            {
                // Create the dialog
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    FileName = fileBrowserModel.DefaultName,
                    Filter = fileBrowserModel.ExtensionFilter,
                    InitialDirectory = fileBrowserModel.DefaultDirectory,
                    Multiselect = fileBrowserModel.MultiSelection,
                    Title = fileBrowserModel.Title ?? "Select a file"
                };

                // Show the dialog and get the result
                bool canceled = openFileDialog.ShowDialog() != true;

                RL.Logger?.LogTraceSource(canceled
                    ? "The browse file dialog was canceled by the user"
                    : $"The browse file dialog returned the selected file paths {openFileDialog.FileNames.JoinItems(", ")}");

                // Return the result
                return Task.FromResult(new FileBrowserResult()
                {
                    CanceledByUser = canceled,
                    SelectedFile = openFileDialog.FileName,
                    SelectedFiles = openFileDialog.FileNames.Select(x => new FileSystemPath(x))
                });
            });
        }

        /// <summary>
        /// Allows the user to browse for a location and chose a name for a file to save
        /// </summary>
        /// <param name="saveFileModel">The save file browser information</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The save file result</returns>
        public virtual Task<SaveFileResult> SaveFileAsync(SaveFileViewModel saveFileModel, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (LogRequests)
                RL.Logger?.LogTraceSource($"A save file dialog was opened with the title of: {saveFileModel.Title}", origin: origin, filePath: filePath, lineNumber: lineNumber);

            return Application.Current.Dispatcher.Invoke(() =>
            {
                // Create the dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    FileName = saveFileModel.DefaultName,
                    Filter = saveFileModel.Extensions,
                    InitialDirectory = saveFileModel.DefaultDirectory,
                    Title = saveFileModel.Title ?? "Save file"
                };

                // Show the dialog and get the result
                bool canceled = saveFileDialog.ShowDialog() != true;

                RL.Logger?.LogTraceSource(canceled
                    ? "The save file dialog was canceled by the user"
                    : $"The save file dialog returned the selected file path {saveFileDialog.FileName}");

                // Return the result
                return Task.FromResult(new SaveFileResult()
                {
                    CanceledByUser = canceled,
                    SelectedFileLocation = saveFileDialog.FileName
                });
            });
        }

        /// <summary>
        /// Allows the user to browse for a drive
        /// </summary>
        /// <param name="driveBrowserModel">The drive browser information</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The browse drive result</returns>
        public virtual async Task<DriveBrowserResult> BrowseDriveAsync(DriveBrowserViewModel driveBrowserModel, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (Application.Current.Dispatcher == null)
                throw new Exception("A drive selection dialog can not be shown when the application dispatcher is null");

            if (LogRequests)
                RL.Logger?.LogTraceSource($"A browse drive dialog was opened with the title of: {driveBrowserModel.Title}", origin: origin, filePath: filePath, lineNumber: lineNumber);

            // Create the dialog
            var driveSelectionDialog = Application.Current.Dispatcher.Invoke(() => new DriveSelectionDialog(driveBrowserModel));

            // Show the dialog and get the result
            var result = await driveSelectionDialog.ShowDialogAsync();

            RL.Logger?.LogTraceSource(result.CanceledByUser
                ? "The browse drive dialog was canceled by the user"
                : $"The browse drive dialog returned the selected drive paths {result.SelectedDrives.JoinItems(", ")}");

            // Return the result
            return result;
        }
    }
}
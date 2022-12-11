#nullable disable
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The default <see cref="IBrowseUIManager"/> for WPF
/// </summary>
public class RCPBrowseUIManager : IBrowseUIManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Indicates if the browse requests should be logged
    /// </summary>
    public virtual bool LogRequests { get; set; }

    /// <summary>
    /// Allows the user to browse for a directory
    /// </summary>
    /// <param name="directoryBrowserModel">The directory browser information</param>
    /// <returns>The directory browser result</returns>
    public virtual Task<DirectoryBrowserResult> BrowseDirectoryAsync(DirectoryBrowserViewModel directoryBrowserModel)
    {
        if (LogRequests)
            Logger.Trace("A browse directory dialog was opened with the title of: {0}", directoryBrowserModel.Title);

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
            var dialogResult = dialog.ShowDialog(Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive));

            var result = dialogResult != CommonFileDialogResult.Ok ? new DirectoryBrowserResult()
            {
                CanceledByUser = true
            } : new DirectoryBrowserResult()
            {
                CanceledByUser = false,
                SelectedDirectory = dialog.FileName,
                SelectedDirectories = dialog.FileNames.Select(x => new FileSystemPath(x))
            };

            Logger.Trace(result.CanceledByUser
                ? "The browse directory dialog was canceled by the user"
                : $"The browse directory dialog returned the selected directory paths {result.SelectedDirectories.JoinItems(", ")}");

            return Task.FromResult(result);
        });
    }

    /// <summary>
    /// Allows the user to browse for a file
    /// </summary>
    /// <param name="fileBrowserModel">The file browser information</param>
    /// <returns>The file browser result</returns>
    public virtual Task<FileBrowserResult> BrowseFileAsync(FileBrowserViewModel fileBrowserModel)
    {
        if (LogRequests)
            Logger.Trace("A browse file dialog was opened with the title of: {0}", fileBrowserModel.Title);

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

            Logger.Trace(canceled
                ? "The browse file dialog was canceled by the user"
                : $"The browse file dialog returned the selected file paths {openFileDialog.FileNames.JoinItems(", ")}");

            // Return the result
            return Task.FromResult(new FileBrowserResult()
            {
                CanceledByUser = canceled,
                SelectedFile = openFileDialog.FileName,
                SelectedFiles = openFileDialog.FileNames.Select(x => new FileSystemPath(x)).ToArray()
            });
        });
    }

    /// <summary>
    /// Allows the user to browse for a location and chose a name for a file to save
    /// </summary>
    /// <param name="saveFileModel">The save file browser information</param>
    /// <returns>The save file result</returns>
    public virtual Task<SaveFileResult> SaveFileAsync(SaveFileViewModel saveFileModel)
    {
        if (LogRequests)
            Logger.Trace("A save file dialog was opened with the title of: {0}", saveFileModel.Title);

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

            Logger.Trace(canceled
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
}
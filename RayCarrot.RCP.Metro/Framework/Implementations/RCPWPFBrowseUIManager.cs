using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.WPF;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The browse UI manager for the Rayman Control Panel
    /// </summary>
    public class RCPWPFBrowseUIManager : DefaultWPFBrowseUIManager
    {
        /// <summary>
        /// The implementation for allowing the user to browse for a directory
        /// </summary>
        /// <param name="directoryBrowserModel">The directory browser information</param>
        /// <returns>The directory browser result</returns>
        protected override Task<DirectoryBrowserResult> BrowseDirectoryImplementationAsync(DirectoryBrowserViewModel directoryBrowserModel)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = directoryBrowserModel.Title;
                dialog.AllowNonFileSystemItems = false;
                dialog.IsFolderPicker = true;
                dialog.Multiselect = directoryBrowserModel.MultiSelection;
                dialog.DefaultDirectory = directoryBrowserModel.DefaultDirectory;
                dialog.DefaultFileName = directoryBrowserModel.DefaultName;
                dialog.EnsureFileExists = true;
                dialog.EnsurePathExists = true;

                if (RCFRCP.Data.UserLevel >= UserLevel.Technical)
                    dialog.AddPlace(CommonPaths.UserDataBaseDir, FileDialogAddPlaceLocation.Top);

                // Show the dialog
                var result = dialog.ShowDialog(Application.Current.Windows.Cast<Window>().FindItem(x => x.IsActive));

                // Return the result
                return Task.FromResult(result != CommonFileDialogResult.Ok ? new DirectoryBrowserResult()
                {
                    CanceledByUser = true
                } : new DirectoryBrowserResult()
                {
                    CanceledByUser = false,
                    SelectedDirectory = dialog.FileName,
                    SelectedDirectories = dialog.FileNames.Select(x => new FileSystemPath(x))
                });
            }
        }
    }
}
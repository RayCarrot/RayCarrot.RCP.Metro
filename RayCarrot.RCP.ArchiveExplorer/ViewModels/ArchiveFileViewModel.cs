﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.UI;
using RayCarrot.WPF;
    
namespace RayCarrot.RCP.ArchiveExplorer
{
    /// <summary>
    /// View model for a file in an archive
    /// </summary>
    public class ArchiveFileViewModel : BaseViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileData">The file data</param>
        /// <param name="archive">The archive the file belongs to</param>
        public ArchiveFileViewModel(IArchiveFileData fileData, ArchiveViewModel archive)
        {
            // Set properties
            FileData = fileData;
            Archive = archive;
            FileName = FileData.FileName;

            // Create commands
            ExportCommand = new AsyncRelayCommand(ExportFileAsync);
            ImportCommand = new AsyncRelayCommand(ImportFileAsync);
        }

        #endregion

        #region Commands

        public ICommand ExportCommand { get; }

        public ICommand ImportCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The archive the file belongs to
        /// </summary>
        public ArchiveViewModel Archive { get; }

        /// <summary>
        /// The file data
        /// </summary>
        public IArchiveFileData FileData { get; }

        /// <summary>
        /// The image source for the thumbnail
        /// </summary>
        public ImageSource ThumbnailSource { get; set; }

        /// <summary>
        /// The archive file stream
        /// </summary>
        public FileStream ArchiveFileStream => Archive.ArchiveFileStream;

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The info about the file to display
        /// </summary>
        public string FileDisplayInfo => FileData.FileDisplayInfo;

        #endregion

        #region Protected Methods

        protected FileFilterItemCollection GetFileFilterCollection()
        {
            return new FileFilterItemCollection(FileData.SupportedFileExtensions.Select(x => new FileFilterItem($"*{x}", x.Substring(1).ToUpper())));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the thumbnail image source for the file
        /// </summary>
        public void LoadThumbnail()
        {
            // Get the bitmap if the item is an image
            if (FileData is IArchiveImageFileData imgData)
            {
                try
                {
                    // Get the thumbnail
                    var img = imgData.
                        // Get the bitmap image
                        GetBitmap(ArchiveFileStream, 64).
                        // Get an image source from the bitmap
                        ToImageSource();
                    
                    // Freeze the image to avoid thread errors
                    img.Freeze();

                    // Set the image source
                    ThumbnailSource = img;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Getting archive file thumbnail");
                }
            }
            else
            {
                RCFCore.Logger?.LogWarningSource("A thumbnail can currently not be generated for non-image files in archives");
            }
        }

        /// <summary>
        /// Exports the file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ExportFileAsync()
        {
            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Run as a task
                await Task.Run(async () =>
                {
                    // Get the output path
                    var result = await RCFUI.BrowseUI.SaveFileAsync(new SaveFileViewModel()
                    {
                        Title = Resources.Archive_ExportHeader,
                        DefaultName = FileName,
                        Extensions = GetFileFilterCollection().ToString()
                    });

                    if (result.CanceledByUser)
                        return;

                    Archive.DisplayStatus = String.Format(Resources.Archive_ExportingFileStatus, FileName);

                    try
                    {
                        // Export the file
                        await FileData.ExportFileAsync(ArchiveFileStream, result.SelectedFileLocation, result.SelectedFileLocation.FileExtension);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Exporting archive file", FileName);

                        await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ExportError, FileName));

                        return;
                    }
                    finally
                    {
                        Archive.DisplayStatus = String.Empty;
                    }

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ExportFileSuccess);
                });
            }
        }

        /// <summary>
        /// Imports a file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportFileAsync()
        {
            // Run as a load operation
            using (Archive.LoadOperation.Run())
            {
                // Run as a task
                await Task.Run(async () =>
                {
                    // Get the file
                    var result = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                    {
                        Title = Resources.Archive_ImportFileHeader,
                        ExtensionFilter = GetFileFilterCollection().CombineAll(Resources.Archive_FileSelectionGroupName).ToString()
                    });

                    if (result.CanceledByUser)
                        return;

                    Archive.DisplayStatus = String.Format(Resources.Archive_ImportingFileStatus, FileName);

                    try
                    {
                        // Import the file
                        var succeeded = await FileData.ImportFileAsync(ArchiveFileStream, result.SelectedFile);

                        // Make sure it succeeded
                        if (!succeeded)
                            return;
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Importing archive file", FileName);

                        await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Archive_ImportError, result.SelectedFile.Name));

                        return;
                    }
                    finally
                    {
                        Archive.DisplayStatus = String.Empty;
                    }

                    // Update the archive
                    await Archive.UpdateArchiveAsync();

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Archive_ImportFileSuccess);
                });
            }
        }

        public void Dispose()
        {
            // Delete temp file
            RCFRCPC.File.DeleteFile(FileData.PendingImportTempPath);
        }

        #endregion
    }
}
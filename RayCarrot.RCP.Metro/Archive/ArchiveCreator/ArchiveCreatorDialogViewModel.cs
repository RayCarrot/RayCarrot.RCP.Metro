using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for an archive creator dialog
    /// </summary>
    public class ArchiveCreatorDialogViewModel : UserInputViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="manager">The manager</param>
        public ArchiveCreatorDialogViewModel(IArchiveCreatorDataManager manager)
        {
            // Set properties
            Title = Resources.Archive_CreateHeader;
            Manager = manager;
        }

        #endregion

        #region Private Fields

        private FileSystemPath _inputDirectory;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current status to display
        /// </summary>
        public string DisplayStatus { get; set; }

        /// <summary>
        /// The manager
        /// </summary>
        public IArchiveCreatorDataManager Manager { get; }

        /// <summary>
        /// Indicates if the creator tool is loading
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// The selected input directory
        /// </summary>
        public FileSystemPath InputDirectory
        {
            get => _inputDirectory;
            set
            {
                _inputDirectory = value;

                if (InputDirectory.DirectoryExists && OutputFile.FullPath.IsNullOrWhiteSpace())
                    OutputFile = (InputDirectory.Parent + Manager.DefaultArchiveFileName).GetNonExistingFileName();
            }
        }

        /// <summary>
        /// The selected output file
        /// </summary>
        public FileSystemPath OutputFile { get; set; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sets the display status
        /// </summary>
        /// <param name="status">The status to display</param>
        public void SetDisplayStatus(string status)
        {
            DisplayStatus = status;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an archive
        /// </summary>
        /// <returns>True if the archive was successfully created, otherwise false</returns>
        public async Task<bool> CreateArchiveAsync()
        {
            try
            {
                if (IsLoading)
                    return false;

                IsLoading = true;

                return await Task.Run(async () =>
                {
                    // Make sure the input directory exists
                    if (!InputDirectory.DirectoryExists)
                    {
                        await RCFUI.MessageUI.DisplayMessageAsync(Resources.Archive_CreateErrorInputNotFound, MessageType.Error);

                        return false;
                    }

                    // Get the archive data from the files
                    var archiveData = Manager.GetArchive(Directory.GetFiles(InputDirectory, "*", SearchOption.AllDirectories).Select(file => file - InputDirectory));

                    // Get the import data
                    var importData = archiveData.FileEntries.Select(x => new ArchiveImportData(x.FileEntry, y =>
                    {
                        // Get the file path
                        var filePath = InputDirectory + x.RelativeImportFilePath;

                        SetDisplayStatus(String.Format(Resources.Archive_ImportingFileStatus, Path.GetFileName(filePath)));

                        // Return the encoded file
                        return Manager.EncodeFile(File.ReadAllBytes(filePath), x.FileEntry);
                    })).ToArray();

                    // Open the output file
                    using var outputStream = File.Open(OutputFile, FileMode.Create, FileAccess.Write);

                    // Update the archive
                    Manager.UpdateArchive(archiveData.Archive, outputStream, importData);

                    DisplayStatus = String.Empty;

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Archive_CreateSuccess, importData.Length));

                    return true;
                });
            }
            catch (Exception ex)
            {
                ex.HandleError("Creating archive", Manager);

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CreateError);

                return false;
            }
            finally
            {
                IsLoading = false;
                DisplayStatus = String.Empty;
            }
        }

        #endregion
    }
}
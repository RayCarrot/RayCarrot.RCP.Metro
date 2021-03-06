﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.WPF;

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
        public ArchiveCreatorDialogViewModel(IArchiveDataManager manager)
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
        public IArchiveDataManager Manager { get; }

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

        #region Event Handlers

        private void Manager_OnWritingFileToArchive(object sender, ValueEventArgs<ArchiveFileItem> e)
        {
            SetDisplayStatus(String.Format(Resources.Archive_CreationFileStatus, e.Value.FileName));
        }

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
                    ArchiveFileItem[] archiveFiles = null;

                    try
                    {
                        Manager.OnWritingFileToArchive += Manager_OnWritingFileToArchive;

                        // Make sure the input directory exists
                        if (!InputDirectory.DirectoryExists)
                        {
                            await Services.MessageUI.DisplayMessageAsync(Resources.Archive_CreateErrorInputNotFound, MessageType.Error);

                            return false;
                        }

                        // Create a new archive
                        var archive = Manager.CreateArchive();

                        var inputFiles = InputDirectory.
                            GetDirectoryInfo().
                            GetFiles("*", Manager.CanModifyDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).
                            Where(x => !x.Attributes.HasFlag(FileAttributes.System)).
                            Select(x => new FileSystemPath(x.FullName)).
                            ToArray();

                        archiveFiles = inputFiles.Select(x =>
                        {
                            var relativePath = x - InputDirectory;
                            var dir = relativePath.Parent.FullPath.Replace(Path.DirectorySeparatorChar, Manager.PathSeparatorCharacter);
                            var file = relativePath.Name;

                            var archiveEntry = Manager.GetNewFileEntry(archive, dir, file);

                            var archiveFileItem = new ArchiveFileItem(Manager, file, dir, archiveEntry);

                            // IDEA: If not encoded there's no need to copy the stream, instead just use origin file

                            // Open the file to be imported
                            using var inputStream = File.OpenRead(x);

                            // Get the temp stream to store the pending import data
                            archiveFileItem.SetPendingImport(File.Create(Path.GetTempFileName()));

                            // Encode the data to the pending import stream
                            Manager.EncodeFile(inputStream, archiveFileItem.PendingImport, archiveEntry);

                            // If no data was encoded we copy over the decoded data
                            if (archiveFileItem.PendingImport.Length == 0)
                                inputStream.CopyTo(archiveFileItem.PendingImport);

                            return archiveFileItem;
                        }).ToArray();

                        // Open the output file
                        using var outputStream = File.Open(OutputFile, FileMode.Create, FileAccess.Write);

                        // Write the archive
                        Manager.WriteArchive(null, archive, outputStream, archiveFiles);
                    }
                    finally
                    {
                        archiveFiles?.DisposeAll();
                        Manager.OnWritingFileToArchive -= Manager_OnWritingFileToArchive;
                        DisplayStatus = String.Empty;
                    }

                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Archive_CreateSuccess, archiveFiles.Length));

                    return true;
                });
            }
            catch (Exception ex)
            {
                ex.HandleError("Creating archive", Manager);

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CreateError);

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro.Archive;

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
        LoaderViewModel = new LoaderViewModel();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private FileSystemPath _inputDirectory;

    #endregion

    #region Public Properties

    /// <summary>
    /// The manager
    /// </summary>
    public IArchiveDataManager Manager { get; }

    public LoaderViewModel LoaderViewModel { get; }

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

    #region Public Methods

    /// <summary>
    /// Creates an archive
    /// </summary>
    /// <returns>True if the archive was successfully created, otherwise false</returns>
    public async Task<bool> CreateArchiveAsync()
    {
        using (LoadState state = await LoaderViewModel.RunAsync(Resources.Archive_CreateStatusPacking, canCancel: true))
        {
            try
            {
                return await Task.Run(async () =>
                {
                    List<FileItem> archiveFiles = new();
                    CancellationToken cancellationToken = state.CancellationToken;
                    bool createdFile = false;

                    try
                    {
                        // Make sure the input directory exists
                        if (!InputDirectory.DirectoryExists)
                        {
                            await Services.MessageUI.DisplayMessageAsync(Resources.Archive_CreateErrorInputNotFound,
                                MessageType.Error);

                            return false;
                        }

                        // Create a new archive
                        object archive = Manager.CreateArchive();

                        // Add each file
                        SearchOption searchOption = Manager.CanModifyDirectories
                            ? SearchOption.AllDirectories
                            : SearchOption.TopDirectoryOnly;
                        foreach (FileSystemPath inputFile in Directory.EnumerateFiles(InputDirectory, "*", searchOption))
                        {
                            // Ignore system files
                            if (inputFile.GetFileInfo().Attributes.HasFlag(FileAttributes.System))
                                continue;

                            cancellationToken.ThrowIfCancellationRequested();

                            FileSystemPath relativePath = inputFile - InputDirectory;
                            string dir = relativePath.Parent.FullPath.Replace(Path.DirectorySeparatorChar,
                                Manager.PathSeparatorCharacter);
                            string file = relativePath.Name;

                            object archiveEntry = Manager.GetNewFileEntry(archive, dir, file);

                            FileItem fileItem = new(Manager, file, dir, archiveEntry);

                            // IDEA: If not encoded there's no need to copy the stream, instead just use origin file

                            // Open the file to be imported
                            using FileStream inputStream = File.OpenRead(inputFile);

                            fileItem.SetPendingImport();

                            // Encode the data to the pending import stream
                            Manager.EncodeFile(inputStream, fileItem.PendingImport, archiveEntry);

                            // If no data was encoded we copy over the decoded data
                            if (fileItem.PendingImport.Length == 0)
                                inputStream.CopyTo(fileItem.PendingImport);

                            archiveFiles.Add(fileItem);
                        }

                        // Open the output file
                        using ArchiveFileStream outputStream =
                            new(File.Open(OutputFile, FileMode.Create, FileAccess.Write), OutputFile.Name, true);

                        createdFile = true;

                        // Write the archive
                        // ReSharper disable once AccessToDisposedClosure
                        Manager.WriteArchive(null, archive, outputStream, archiveFiles, x => state.SetProgress(x),
                            cancellationToken);
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Trace(ex, "Cancelled creating archive");

                        try
                        {
                            if (createdFile)
                                OutputFile.DeleteFile();
                        }
                        catch (Exception ex2)
                        {
                            Logger.Warn(ex2, "Deleting not fully written archive output file");
                        }
                        
                        return false;
                    }
                    finally
                    {
                        archiveFiles.DisposeAll();
                    }

                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Archive_CreateSuccess, archiveFiles.Count));

                    return true;
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Creating archive using manager {0}", Manager);

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CreateError);

                return false;
            }
        }
    }

    #endregion
}
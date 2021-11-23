using Nito.AsyncEx;
using RayCarrot.IO;
using NLog;
using RayCarrot.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for an archive from a file
/// </summary>
public class ArchiveViewModel : ArchiveDirectoryViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="filePath">The file path for the archive</param>
    /// <param name="manager">The archive data manager</param>
    /// <param name="loadOperation">The operation to use when running an async operation which needs to load</param>
    /// <param name="explorerDialogViewModel">The explorer dialog view model</param>
    /// <param name="isDuplicateName">Indicates if the name of the archive matches the name of another loaded archive</param>
    public ArchiveViewModel(FileSystemPath filePath, IArchiveDataManager manager, Operation loadOperation, ArchiveExplorerDialogViewModel explorerDialogViewModel, bool isDuplicateName) : base(filePath.Name)
    {
        Logger.Info("An archive view model is being created for {0}", filePath.Name);

        // Set properties
        FilePath = filePath;
        Manager = manager;
        LoadOperation = loadOperation;
        ExplorerDialogViewModel = explorerDialogViewModel;
        IsDuplicateName = isDuplicateName;
        ThumbnailCache = new ArchiveThumbnailCache();

        // Create commands
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
            
        // Create the file stream
        ArchiveFileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Protected Properties

    /// <summary>
    /// The current amount of modified files
    /// </summary>
    protected int ModifiedFilesCount { get; set; }

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }

    public ICommand OpenLocationCommand { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the name of the archive matches the name of another loaded archive
    /// </summary>
    public bool IsDuplicateName { get; }

    /// <summary>
    /// The explorer dialog view model
    /// </summary>
    public ArchiveExplorerDialogViewModel ExplorerDialogViewModel { get; }

    /// <summary>
    /// The archive file generator
    /// </summary>
    public IDisposable ArchiveFileGenerator { get; set; }

    /// <summary>
    /// The operation to use when running an async operation which needs to load
    /// </summary>
    public Operation LoadOperation { get; }

    /// <summary>
    /// The archive the directory belongs to
    /// </summary>
    public override ArchiveViewModel Archive => this;

    /// <summary>
    /// The archive file stream
    /// </summary>
    public FileStream ArchiveFileStream { get; protected set; }

    /// <summary>
    /// The data for the loaded archive
    /// </summary>
    public object ArchiveData { get; set; }

    /// <summary>
    /// The lock to use when accessing the archive stream
    /// </summary>
    public AsyncLock ArchiveLock => ExplorerDialogViewModel.ArchiveLock;

    /// <summary>
    /// The file path for the archive
    /// </summary>
    public FileSystemPath FilePath { get; }

    /// <summary>
    /// The description to display
    /// </summary>
    public override string DisplayDescription => FilePath.FullPath;

    /// <summary>
    /// The name of the item to display
    /// </summary>
    public override string DisplayName => IsDuplicateName ? $"{(FileSystemPath)FilePath.Parent.Name + FilePath.Name}" : FilePath.Name;

    /// <summary>
    /// The name of the item
    /// </summary>
    public string Name => FilePath.Name;

    /// <summary>
    /// The name of the directory to use when exporting
    /// </summary>
    public override string ExportDirName => FilePath.RemoveFileExtension().Name;

    /// <summary>
    /// The archive data manager
    /// </summary>
    public IArchiveDataManager Manager { get; }

    /// <summary>
    /// Gets the currently selected item
    /// </summary>
    public ArchiveDirectoryViewModel SelectedItem => this.GetAllChildren<ArchiveDirectoryViewModel>(true).FirstOrDefault(x => x.IsSelected);
       
    /// <summary>
    /// The text to display for the save prompt if there are modified files
    /// </summary>
    public string ModifiedFilesDisplayText { get; protected set; }

    /// <summary>
    /// Indicates if there are modified files
    /// </summary>
    public bool HasModifiedFiles { get; protected set; }

    public ArchiveThumbnailCache ThumbnailCache { get; }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Clears and disposes every item
    /// </summary>
    protected void ClearAndDisposeItems()
    {
        Logger.Info("The archive items have been cleared and disposed");

        // Dispose every directory
        this.GetAllChildren<ArchiveDirectoryViewModel>().DisposeAll();

        // Dispose files
        Files.DisposeAll();

        // Clear the items
        Clear();
        Files.Clear();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the display status
    /// </summary>
    /// <param name="status">The status to display</param>
    public void SetDisplayStatus(string status)
    {
        ExplorerDialogViewModel.DisplayStatus = status;
    }

    /// <summary>
    /// Loads the archive
    /// </summary>
    public void LoadArchive()
    {
        Logger.Info("The archive {0} is being loaded", DisplayName);

        // Clear existing items
        ClearAndDisposeItems();

        // Indicate that no files have pending edits
        ModifiedFilesCount = 0;
        HasModifiedFiles = false;

        // Load the archive data
        ArchiveData = Manager.LoadArchive(ArchiveFileStream);

        // Load the archive
        var data = Manager.LoadArchiveData(ArchiveData, ArchiveFileStream, FilePath.Name);

        // Dispose the current generator
        ArchiveFileGenerator?.Dispose();

        // Save the generator
        ArchiveFileGenerator = data.Generator;

        // Add each directory
        foreach (var dir in data.Directories)
        {
            // Check if it's the root directory
            if (dir.DirectoryName == String.Empty)
            {
                // Add the files
                Files.AddRange(dir.Files.Select(x => new ArchiveFileViewModel(x, this)));

                continue;
            }

            // Keep track of the previous item
            ArchiveDirectoryViewModel prevItem = this;

            // Enumerate each sub directory
            foreach (string subDir in dir.DirectoryName.Trim(Manager.PathSeparatorCharacter).Split(Manager.PathSeparatorCharacter))
            {
                // Set the previous item and create the item if it doesn't already exist
                prevItem = prevItem.FirstOrDefault(x => x.ID == subDir) ?? prevItem.Add(subDir);
            }

            // Add the files
            prevItem.Files.AddRange(dir.Files.Select(x => new ArchiveFileViewModel(x, prevItem)));
        }
    }

    /// <summary>
    /// Saves any pending changes to the archive and reloads it
    /// </summary>
    /// <returns>The task</returns>
    public async Task SaveAsync()
    {
        Logger.Info("The archive {0} is being repacked", DisplayName);

        // Run as a load operation
        using (Archive.LoadOperation.Run())
        {
            // Lock the access to the archive
            using (await Archive.ArchiveLock.LockAsync())
            {
                // Find the selected item ID
                var selected = SelectedItem.FullID;

                // Run as a task
                await Task.Run(async () =>
                {
                    // Stop file initialization
                    if (ExplorerDialogViewModel.IsInitializingFiles)
                        ExplorerDialogViewModel.CancelInitializeFiles = true;

                    Archive.SetDisplayStatus(String.Format(Resources.Archive_RepackingStatus, DisplayName));

                    try
                    {
                        // Get a temporary file path to write to
                        using var tempOutputFile = new TempFile(false);

                        // Create the file and get the stream
                        using (var outputStream = File.Create(tempOutputFile.TempPath))
                        {
                            // Write to the stream
                            Manager.WriteArchive(ArchiveFileGenerator, ArchiveData, outputStream, this.GetAllChildren<ArchiveDirectoryViewModel>(true).SelectMany(x => x.Files).Select(x => x.FileData).ToArray());
                        }

                        // Dispose the archive file stream
                        ArchiveFileStream.Dispose();

                        ArchiveData = null;
                        ArchiveFileStream = null;

                        // If the operation succeeded, replace the archive file with the temporary output
                        Services.File.MoveFile(tempOutputFile.TempPath, FilePath, true);

                        // Re-open the file stream
                        ArchiveFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Repacking archive {0}", DisplayName);

                        await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_RepackError);

                        // Re-open the file stream if closed
                        if (ArchiveFileStream == null)
                            ArchiveFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    }
                });

                // Reload the archive
                LoadArchive();

                // Get the previously selected item
                var previouslySelectedItem = this.GetAllChildren<ArchiveDirectoryViewModel>(true).FirstOrDefault(x => x.FullID.SequenceEqual(selected));

                // Load the previously selected directory if it still exists
                if (previouslySelectedItem != null)
                    ExplorerDialogViewModel.LoadDirectory(previouslySelectedItem);

                Archive.SetDisplayStatus(String.Empty);
            }
        }
    }

    /// <summary>
    /// Opens the location of the archive
    /// </summary>
    /// <returns>The task</returns>
    public async Task OpenLocationAsync()
    {
        // Open the location
        await Services.File.OpenExplorerLocationAsync(FilePath);

        Logger.Trace("The archive {0} location was opened", DisplayName);
    }

    /// <summary>
    /// Adds the specified number of new files as being modified
    /// </summary>
    /// <param name="count">The number of new modified files to add</param>
    /// <param name="allow0">Indicates if a count of 0 should be allowed</param>
    public void AddModifiedFiles(int count = 1, bool allow0 = false)
    {
        if (count == 0 && !allow0)
            return;

        // Increment by the count
        ModifiedFilesCount += count;

        // Update text
        ModifiedFilesDisplayText = $"{ModifiedFilesCount} files have been modified in {DisplayName}";

        // Update boolean
        HasModifiedFiles = true;
    }

    /// <summary>
    /// Disposes the archive and its folders and files
    /// </summary>
    public override void Dispose()
    {
        // Cancel refreshing thumbnails
        ExplorerDialogViewModel.CancelInitializeFiles = true;

        // Dispose base class
        base.Dispose();

        // Dispose the stream
        ArchiveFileStream?.Dispose();

        // Dispose every directory
        ClearAndDisposeItems();

        // Dispose the generator
        ArchiveFileGenerator?.Dispose();

        // Dispose the cache
        ThumbnailCache?.Dispose();

        Logger.Info("The archive {0} has been disposed", DisplayName);
    }

    #endregion
}
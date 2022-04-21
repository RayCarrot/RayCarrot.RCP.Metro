using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using AutoCompleteTextBox.Editors;
using ByteSizeLib;
using Nito.AsyncEx;
using NLog;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// View model for an archive explorer dialog
/// </summary>
public class ArchiveExplorerDialogViewModel : UserInputViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="manager">The archive data manager</param>
    /// <param name="filePaths">The archive file paths</param>
    /// <param name="onRepackAction">An optional action to call after repacking an archive</param>
    public ArchiveExplorerDialogViewModel(IArchiveDataManager manager, FileSystemPath[] filePaths, Func<FileSystemPath, Task>? onRepackAction = null)
    {
        OnRepackAction = onRepackAction;
        // Create commands
        NavigateToAddressCommand = new RelayCommand(() => LoadDirectory(CurrentDirectoryAddress));
        DeleteSelectedDirCommand = new AsyncRelayCommand(DeleteSelectedDirAsync);

        // Set properties
        CurrentDirectorySuggestions = new ObservableCollection<string>();
        StatusBarItems = new ObservableCollection<LocalizedString>();
        SearchProvider = new BaseSuggestionProvider(SearchForEntries);

        BindingOperations.EnableCollectionSynchronization(StatusBarItems, Application.Current);

        // TODO: Do not load the archives in the constructor! Create a separate init method or similar.

        try
        {
            // Set the default title
            Title = Resources.Archive_Title;

            // Get the manager
            Manager = manager;

            // Create the load action
            Operation load = new(() => IsLoading = true, () => IsLoading = false);

            // Get the archives
            Archives = filePaths.Select(x => new ArchiveViewModel(x, manager, load, this, filePaths.Any(f => f != x && f.Name == x.Name))).ToArray();

            // Set the archive lock
            ArchiveLock = new AsyncLock();

            Logger.Info("The Archive Explorer is loading with {0} archives", Archives.Length);

            // Make sure we got an archive
            if (!Archives.Any())
                throw new ArgumentException("At least one archive path needs to be available");

            // Lock when accessing the archive
            using (ArchiveLock.Lock())
            {
                // Load each archive
                foreach (var archive in Archives)
                    archive.LoadArchive();
            }

            // Select and expand the first item
            Archives.First().IsSelected = true;
            Archives.First().IsExpanded = true;
        }
        catch
        {
            // Make sure the view model gets disposed
            Dispose();

            throw;
        }
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private string? _currentDirectoryAddress;
    private IArchiveFileSystemEntryViewModel? _selectedSearchEntry;

    #endregion

    #region Constants

    private const char SourceSeparator = ':';
    private const StringComparison StringValueComparison = StringComparison.InvariantCultureIgnoreCase;

    #endregion

    #region Commands

    public ICommand NavigateToAddressCommand { get; }
    public ICommand DeleteSelectedDirCommand { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// An optional action to call after repacking an archive
    /// </summary>
    public Func<FileSystemPath, Task>? OnRepackAction { get; }

    /// <summary>
    /// Enumerates the directories in all available archives
    /// </summary>
    public IEnumerable<DirectoryViewModel> EnumerateDirectories => Archives.Select(y => ((DirectoryViewModel)y).GetAllChildren(true)).SelectMany(y => y);

    /// <summary>
    /// Enumerates the files in all available archives
    /// </summary>
    public IEnumerable<FileViewModel> EnumerateFiles => EnumerateDirectories.SelectMany(y => y.Files);

    /// <summary>
    /// Enumerates the entries in all available archives
    /// </summary>
    public IEnumerable<IArchiveFileSystemEntryViewModel> EnumerateEntries
    {
        get
        {
            // Get the directories
            var dirs = EnumerateDirectories.ToArray();

            return dirs.Cast<IArchiveFileSystemEntryViewModel>().Concat(dirs.SelectMany(y => y.Files));
        }
    }

    /// <summary>
    /// Indicates if files are being initialized
    /// </summary>
    public bool IsInitializingFiles { get; set; }

    /// <summary>
    /// Indicates if initializing files should be canceled
    /// </summary>
    public bool CancelInitializeFiles { get; set; }

    /// <summary>
    /// Indicates if a process is running, such as importing/exporting
    /// </summary>
    public bool IsLoading { get; set; }

    /// <summary>
    /// The directories
    /// </summary>
    public ArchiveViewModel[] Archives { get; }

    /// <summary>
    /// The archive data manager
    /// </summary>
    public IArchiveDataManager Manager { get; }

    /// <summary>
    /// The current status to display
    /// </summary>
    public string? DisplayStatus { get; set; }

    /// <summary>
    /// The lock to use when accessing any archive stream
    /// </summary>
    public AsyncLock ArchiveLock { get; }

    /// <summary>
    /// The current directory address
    /// </summary>
    public string CurrentDirectoryAddress
    {
        get => _currentDirectoryAddress ?? String.Empty;
        set
        {
            _currentDirectoryAddress = value;
                
            RefreshDirectorySuggestions();
        }
    }

    /// <summary>
    /// The search provider
    /// </summary>
    public BaseSuggestionProvider SearchProvider { get; }

    /// <summary>
    /// The currently selected search file
    /// </summary>
    public IArchiveFileSystemEntryViewModel? SelectedSearchEntry
    {
        get => _selectedSearchEntry;
        set
        {
            _selectedSearchEntry = value;

            // If an entry has been selected we navigate to it and then clear the value
            if (SelectedSearchEntry != null)
            {
                NavigateToEntry(SelectedSearchEntry);
                SelectedSearchEntry = null;
            }
        }
    }

    /// <summary>
    /// The currently available directory suggestions
    /// </summary>
    public ObservableCollection<string> CurrentDirectorySuggestions { get; }

    /// <summary>
    /// The currently selected directory
    /// </summary>
    public DirectoryViewModel? SelectedDir { get; set; }

    /// <summary>
    /// Indicates if multiple files are selected
    /// </summary>
    public bool AreMultipleFilesSelected { get; set; }

    /// <summary>
    /// The items to show for the status bar
    /// </summary>
    public ObservableCollection<LocalizedString> StatusBarItems { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Searches for archive entries using the specified search string
    /// </summary>
    /// <param name="search">The search string</param>
    /// <returns>The matching entries</returns>
    public IEnumerable<IArchiveFileSystemEntryViewModel> SearchForEntries(string search)
    {
        return EnumerateEntries.Where(y => y.DisplayName.IndexOf(search, StringValueComparison) > -1);
    }

    /// <summary>
    /// Navigates to the specified archive entry
    /// </summary>
    /// <param name="entry">The entry to navigate to</param>
    public void NavigateToEntry(IArchiveFileSystemEntryViewModel entry)
    {
        // Check the entry type
        if (entry is FileViewModel file)
        {
            LoadDirectory(file.Directory);
            SelectFile(file);
        }
        else if (entry is DirectoryViewModel dir)
        {
            LoadDirectory(dir);
            entry.IsSelected = true;
        }
        else
        {
            Logger.Warn("Attempted to navigate to unsupported archive entry type {0}", entry);
        }
    }

    /// <summary>
    /// Gets the formatted address for a directory based on a source and path
    /// </summary>
    /// <param name="source">The directory source</param>
    /// <param name="path">The directory path</param>
    /// <returns>The directory address</returns>
    public string GetDirectoryAddress(string source, string? path)
    {
        return $"{source}{SourceSeparator}{Manager.PathSeparatorCharacter}{path}";
    }

    /// <summary>
    /// Gets the formatted address for a directory
    /// </summary>
    /// <param name="dir">The directory to get the address for</param>
    /// <returns>The directory address</returns>
    public string GetDirectoryAddress(DirectoryViewModel dir)
    {
        string source = dir.Archive.DisplayName;
        string path = dir.FullPath;

        return GetDirectoryAddress(source, path);
    }

    /// <summary>
    /// Gets the directory for the specified address
    /// </summary>
    /// <param name="address">The address</param>
    /// <returns>The directory, or null if no match was found</returns>
    public DirectoryViewModel? GetDirectory(string address)
    {
        // Find the source separator
        int sourceIndex = address.IndexOf(SourceSeparator);

        // If there is no source separator the address is invalid
        if (sourceIndex == -1)
            return null;

        // Get the source
        string source = address.Substring(0, sourceIndex);

        // Find the matching archive based on the source
        DirectoryViewModel? dir = Archives.FirstOrDefault(x => x.DisplayName.Equals(source, StringValueComparison));

        // If none was found it's invalid
        if (dir == null)
            return null;

        // Get all path sections
        string[] paths = address.
            // Ignore the source and source separator
            Substring(sourceIndex + 1).
            // Trim the path separators so the split will give us a correct result
            TrimEnd(Manager.PathSeparatorCharacter).
            // Split the path into sections
            Split(Manager.PathSeparatorCharacter);

        // Enumerate every path section, skipping the initial one (the source)
        foreach (string path in paths.Skip(1))
        {
            // Find the matching sub-directory
            dir = dir.FirstOrDefault(x => x.ID.Equals(path, StringValueComparison));

            if (dir == null)
                return null;
        }

        return dir;
    }

    /// <summary>
    /// Updates the current directory address
    /// </summary>
    public void UpdateAddress()
    {
        _currentDirectoryAddress = SelectedDir == null ? null : GetDirectoryAddress(SelectedDir);
        OnPropertyChanged(nameof(CurrentDirectoryAddress));
    }

    /// <summary>
    /// Refreshes the current directory suggestions
    /// </summary>
    public void RefreshDirectorySuggestions()
    {
        // Clear previous suggestions
        CurrentDirectorySuggestions.Clear();

        // Get the source separator index from the address
        int sourceIndex = CurrentDirectoryAddress.IndexOf(SourceSeparator);

        // If none was found we only include the archives (sources) in the suggestions
        if (sourceIndex == -1)
        {
            CurrentDirectorySuggestions.AddRange(Archives.Select(x => GetDirectoryAddress(x.DisplayName, null)));
        }
        // If a source is specified we search within the source
        else
        {
            int separatorIndex = CurrentDirectoryAddress.LastIndexOf(Manager.PathSeparatorCharacter);
            string parentDir = CurrentDirectoryAddress.Substring(0, separatorIndex);
            DirectoryViewModel? dir = GetDirectory(parentDir);

            if (dir != null)
                CurrentDirectorySuggestions.AddRange(dir.Select(GetDirectoryAddress));
        }
    }

    /// <summary>
    /// Refreshes the status bar
    /// </summary>
    public void RefreshStatusBar()
    {
        // Dispose and clear the previous items
        StatusBarItems.DisposeAll();
        StatusBarItems.Clear();

        // Get the selected directory
        DirectoryViewModel? selectedDir = SelectedDir;

        if (selectedDir == null)
        {
            Logger.Trace("Refreshed the status bar with no selected directory");
            return;
        }

        // Show the file count
        StatusBarItems.Add(new ResourceLocString(nameof(Resources.Archive_Status_FilesCount), selectedDir.Files.Count));

        int selectedFilesCount = 0;
        ByteSize selectedFilesSize = new();
        bool invalidSize = false;

        // Enumerate every selected file
        foreach (FileViewModel file in selectedDir.GetSelectedFiles())
        {
            // Keep track of the count
            selectedFilesCount++;

            if (invalidSize)
                continue;

            // Get the file size
            long? length = Manager.GetFileSize(file.FileData.ArchiveEntry, false);

            // If the file size was invalid we assume the current archive type does not support retrieving the size and thus ignore it
            if (length == null)
                invalidSize = true;
            else
                // Add the size (if multiple files are selected we show the total size)
                selectedFilesSize = selectedFilesSize.AddBytes(length.Value);
        }

        // Show the selected files count if multiple ones are selected
        if (selectedFilesCount > 1)
            StatusBarItems.Add(new ResourceLocString(nameof(Resources.Archive_Status_SelectedFilesCount), selectedFilesCount));

        // Show the total file size for all selected files if any are selected
        if (selectedFilesCount > 0 && !invalidSize)
            StatusBarItems.Add($"{selectedFilesSize}");

        Logger.Trace("Refreshed the status bar");
    }

    /// <summary>
    /// Loads the specified directory
    /// </summary>
    /// <param name="dir">The directory to load</param>
    public void LoadDirectory(DirectoryViewModel dir)
    {
        // Expand the parent items
        DirectoryViewModel? parent = dir;

        while (parent != null)
        {
            // Only expand if there are sub-directories
            if (parent.Any())
                parent.IsExpanded = true;

            // Get the next parent
            parent = parent.Parent;
        }

        // If the item is selected, simply initialize the files, but without awaiting it
        if (dir.IsSelected)
            // TODO: Cleaner way of doing this
            // Run async without awaiting
            ChangeLoadedDirAsync(null, dir).WithoutAwait("Changed loaded directory");
        // Otherwise select the item and let it automatically get initialized
        else
            dir.IsSelected = true;
    }

    /// <summary>
    /// Attempts to load the directory specified by the address
    /// </summary>
    /// <param name="address">The address of the directory to load</param>
    public void LoadDirectory(string address)
    {
        Logger.Debug("Loading directory from address: {0}", address);

        // Get the directory
        DirectoryViewModel? dir = GetDirectory(address);

        // Load the directory if not null
        if (dir != null)
            LoadDirectory(dir);
    }

    /// <summary>
    /// Updates the loaded directory thumbnails
    /// </summary>
    /// <returns>The task</returns>
    public async Task ChangeLoadedDirAsync(DirectoryViewModel? previousDir, DirectoryViewModel newDir)
    {
        if (newDir == null)
            throw new ArgumentNullException(nameof(newDir));

        Logger.Debug("The loaded archive directory is changing from {0} to {1}", previousDir?.DisplayName ?? "NULL", newDir.DisplayName);

        // Stop refreshing thumbnails
        if (IsInitializingFiles)
            CancelInitializeFiles = true;

        // Set the selected directory
        SelectedDir = newDir;

        // Refresh the status bar
        RefreshStatusBar();

        // Update the address bar
        UpdateAddress();

        Logger.Debug("Updating loaded archive dir from {0} to {1}", previousDir?.DisplayName, newDir.DisplayName);

        // Lock the access to the archive
        using (await ArchiveLock.LockAsync())
        {
            try
            {
                // Check if the operation should be canceled
                if (CancelInitializeFiles)
                {
                    Logger.Debug("Canceled initializing files for archive dir {0}", newDir.DisplayName);

                    return;
                }

                // Indicate that we are refreshing the thumbnails
                IsInitializingFiles = true;

                Logger.Debug("Initializing files for archive dir {0}", newDir.DisplayName);

                // Unload the files
                previousDir?.Files.ForEach(x => x.Unload());

                // Initialize files in the new directory
                await Task.Run(() =>
                {
                    // Initialize each file
                    foreach (var x in newDir.Files.
                                 // Copy to an array to avoid an exception when the files refresh
                                 ToArray())
                    {
                        // Check if the operation should be canceled
                        if (CancelInitializeFiles)
                        {
                            Logger.Debug("Canceled initializing files for archive dir {0}", newDir.DisplayName);

                            return;
                        }

                        // Initialize the file
                        x.InitializeFile(thumbnailLoadMode: FileViewModel.ThumbnailLoadMode.LoadThumbnail);
                    }
                });

                // Dispose the context to flush the log
                Manager.Context?.Dispose();

                Logger.Debug("Initialized files for archive dir {0}", newDir.DisplayName);
            }
            finally
            {
                IsInitializingFiles = false;
                CancelInitializeFiles = false;
            }
        }
    }

    /// <summary>
    /// Deletes the selected directory, if one is selected
    /// </summary>
    /// <returns>The task</returns>
    public async Task DeleteSelectedDirAsync()
    {
        if (SelectedDir == null)
            return;

        await SelectedDir.DeleteDirectoryAsync();
    }

    /// <summary>
    /// Selects the specified file
    /// </summary>
    /// <param name="file">The file to select</param>
    /// <param name="resetSelection">Indicates if the file selection should be reset</param>
    public void SelectFile(FileViewModel file, bool resetSelection = true)
    {
        if (resetSelection)
            ResetFileSelection();

        file.IsSelected = true;
    }

    /// <summary>
    /// Clears the current file selection, deselecting all files in the current directory
    /// </summary>
    public void ResetFileSelection()
    {
        if (SelectedDir == null)
            return;

        foreach (FileViewModel file in SelectedDir.Files)
            file.IsSelected = false;
    }

    /// <summary>
    /// Disposes the archives
    /// </summary>
    public void Dispose()
    {
        // Dispose every archive
        Archives?.DisposeAll();

        // Dispose the status bar items
        StatusBarItems?.DisposeAll();

        // Dispose the manager
        Manager.Dispose();

        // Disable collection synchronization
        BindingOperations.DisableCollectionSynchronization(StatusBarItems);
    }

    #endregion

    #region Classes

    /// <summary>
    /// A base suggestion provider
    /// </summary>
    public class BaseSuggestionProvider : ISuggestionProvider
    {
        public BaseSuggestionProvider(Func<string, IEnumerable> getSuggestionsFunc)
        {
            GetSuggestionsFunc = getSuggestionsFunc;
        }

        protected Func<string, IEnumerable> GetSuggestionsFunc { get; }

        public IEnumerable GetSuggestions(string filter) => GetSuggestionsFunc(filter);
    }

    #endregion
}
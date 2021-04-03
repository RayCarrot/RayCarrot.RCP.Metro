using RayCarrot.Common;
using RayCarrot.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoCompleteTextBox.Editors;
using Nito.AsyncEx;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
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
        public ArchiveExplorerDialogViewModel(IArchiveDataManager manager, IEnumerable<FileSystemPath> filePaths)
        {
            // Create commands
            NavigateToAddressCommand = new RelayCommand(NavigateToAddress);

            // Set properties
            CurrentDirectorySuggestions = new ObservableCollection<string>();
            SearchProvider = new BaseSuggestionProvider(x => EnumerateFiles.Where(y => y.FileName.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1));

            try
            {
                // Set the default title
                Title = Resources.Archive_Title;

                // Get the manager
                Manager = manager;

                // Create the load action
                var load = new Operation(() => IsLoading = true, () => IsLoading = false, true);

                // Get the archives
                Archives = filePaths.Select(x => new ArchiveViewModel(x, manager, load, this)).ToArray();

                // Set the archive lock
                ArchiveLock = new AsyncLock();

                RL.Logger?.LogInformationSource($"The Archive Explorer is loading with {Archives.Length} archives");

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

        #region Private Fields

        private string _currentDirectoryAddress;

        private ArchiveFileViewModel _currentSearchFile;

        #endregion

        #region Commands

        public ICommand NavigateToAddressCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Enumerates the files in all available archives
        /// </summary>
        public IEnumerable<ArchiveFileViewModel> EnumerateFiles => Archives.SelectMany(y => ((ArchiveDirectoryViewModel)y).GetAllChildren(true)).SelectMany(y => y.Files);

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
        public string DisplayStatus { get; set; }

        /// <summary>
        /// The lock to use when accessing any archive stream
        /// </summary>
        public AsyncLock ArchiveLock { get; }

        /// <summary>
        /// The current directory address
        /// </summary>
        public string CurrentDirectoryAddress
        {
            get => _currentDirectoryAddress;
            set
            {
                _currentDirectoryAddress = value;

                CurrentDirectorySuggestions.Clear();

                var separatorIndex = CurrentDirectoryAddress.LastIndexOf(Manager.PathSeparatorCharacter);
                if (separatorIndex == -1)
                {
                    CurrentDirectorySuggestions.AddRange(Archives.Select(x => $"{x.DisplayName}:{Manager.PathSeparatorCharacter}"));
                }
                else
                {
                    var parentDir = CurrentDirectoryAddress.Substring(0, separatorIndex);
                    var dir = GetDirectory(parentDir);

                    if (dir != null)
                        CurrentDirectorySuggestions.AddRange(dir.Select(x => $"{x.Archive.DisplayName}:{Manager.PathSeparatorCharacter}{x.FullPath}"));
                }
            }
        }

        /// <summary>
        /// The search provider
        /// </summary>
        public BaseSuggestionProvider SearchProvider { get; }

        /// <summary>
        /// The currently selected search file
        /// </summary>
        public ArchiveFileViewModel CurrentSearchFile
        {
            get => _currentSearchFile;
            set
            {
                _currentSearchFile = value;

                if (CurrentSearchFile != null)
                {
                    LoadDirectory(CurrentSearchFile.ArchiveDirectory);
                    CurrentSearchFile.IsSelected = true;
                    CurrentSearchFile = null;
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
        public ArchiveDirectoryViewModel SelectedDir { get; set; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the directory for the specified address
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns>The directory, or null if no match was found</returns>
        protected ArchiveDirectoryViewModel GetDirectory(string address)
        {
            var paths = address.TrimEnd(Manager.PathSeparatorCharacter).Split(Manager.PathSeparatorCharacter);

            ArchiveDirectoryViewModel dir = Archives.FirstOrDefault(x => x.DisplayName.Equals(paths.FirstOrDefault()?.TrimEnd(':'), StringComparison.OrdinalIgnoreCase));

            if (dir == null)
                return null;

            foreach (var path in paths.Skip(1))
            {
                dir = dir.FirstOrDefault(x => x.ID.Equals(path, StringComparison.OrdinalIgnoreCase));

                if (dir == null)
                    return null;
            }

            return dir;
        }

        /// <summary>
        /// Attempts to load the directory specified by the address
        /// </summary>
        /// <param name="address">The address of the directory to load</param>
        protected void LoadDirectory(string address)
        {
            RL.Logger.LogDebugSource($"Loading directory from address: {address}");

            // Get the directory
            var dir = GetDirectory(address);

            // Load the directory if not null
            if (dir != null)
                LoadDirectory(dir);
        }

        /// <summary>
        /// Updates the current directory address
        /// </summary>
        protected void UpdateAddress()
        {
            CurrentDirectoryAddress = $"{SelectedDir.Archive.DisplayName}:{SelectedDir.Archive.Manager.PathSeparatorCharacter}{SelectedDir.FullPath}";
            OnPropertyChanged(nameof(CurrentDirectoryAddress)); // Temp fix - without this the text box won't update if going back to a parent directory, why? The event gets triggered correctly and the value gets updated
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the specified directory
        /// </summary>
        /// <param name="dir">The directory to load</param>
        public void LoadDirectory(ArchiveDirectoryViewModel dir)
        {
            // Expand the parent items
            var parent = dir;

            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.Parent;
            }

            // If the item is selected, simply initialize the files, but without awaiting it
            if (dir.IsSelected)
                // Run async without awaiting
                _ = ChangeLoadedDirAsync(null, dir);
            // Otherwise select the item and let the thumbnails get automatically reloaded
            else
                dir.IsSelected = true;
        }

        /// <summary>
        /// Updates the loaded directory thumbnails
        /// </summary>
        /// <returns>The task</returns>
        public async Task ChangeLoadedDirAsync(ArchiveDirectoryViewModel previousDir, ArchiveDirectoryViewModel newDir)
        {
            RL.Logger?.LogDebugSource($"The loaded archive directory is changing from {previousDir?.DisplayName ?? "NULL"} to {newDir?.DisplayName ?? "NULL"}");

            // Stop refreshing thumbnails
            if (IsInitializingFiles)
                CancelInitializeFiles = true;

            // Set the selected directory
            SelectedDir = newDir;

            // Update the address bar
            UpdateAddress();

            RL.Logger?.LogDebugSource($"Updating loaded archive dir from {previousDir?.DisplayName} to {newDir.DisplayName}");

            // Lock the access to the archive
            using (await ArchiveLock.LockAsync())
            {
                try
                {
                    // Check if the operation should be canceled
                    if (CancelInitializeFiles)
                    {
                        RL.Logger?.LogDebugSource($"Canceled initializing files for archive dir {newDir.DisplayName}");

                        return;
                    }

                    // Indicate that we are refreshing the thumbnails
                    IsInitializingFiles = true;

                    RL.Logger?.LogDebugSource($"Initializing files for archive dir {newDir.DisplayName}");

                    // Remove all thumbnail image sources from memory
                    previousDir?.Files.ForEach(x =>
                    {
                        x.IsInitialized = false;
                        x.ThumbnailSource = null;
                    });

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
                                RL.Logger?.LogDebugSource($"Canceled initializing files for archive dir {newDir.DisplayName}");

                                return;
                            }

                            // Initialize the file
                            x.InitializeFile();
                        }
                    });

                    RL.Logger?.LogDebugSource($"Initialized files for archive dir {newDir.DisplayName}");
                }
                finally
                {
                    IsInitializingFiles = false;
                    CancelInitializeFiles = false;
                }
            }
        }

        /// <summary>
        /// Attempts to navigate to the currently address
        /// </summary>
        public void NavigateToAddress()
        {
            LoadDirectory(CurrentDirectoryAddress);
            UpdateAddress();
        }

        /// <summary>
        /// Disposes the archives
        /// </summary>
        public void Dispose() => Archives?.DisposeAll();

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
}
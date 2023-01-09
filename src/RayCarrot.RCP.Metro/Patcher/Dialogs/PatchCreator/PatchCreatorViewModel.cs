using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchCreatorViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public PatchCreatorViewModel(params GameInstallation[] gameTargets)
    {
        // Set properties
        ID = PatchMetadata.GenerateID();
        AvailableLocations = new ObservableCollection<AvailableFileLocation>();
        LoaderViewModel = new LoaderViewModel();

        // Set the game targets
        SetGameTargets(gameTargets);

        // Create commands
        EditGameTargetsCommand = new AsyncRelayCommand(EditGameTargetsAsync);
        ImportPatchCommand = new AsyncRelayCommand(ImportPatchAsync);
        BrowseThumbnailCommand = new AsyncRelayCommand(BrowseThumbnailAsync);
        RemoveThumbnailCommand = new RelayCommand(RemoveThumbnail);
        AddFileCommand = new RelayCommand(AddFile);
        AddFromFilesCommand = new AsyncRelayCommand(AddFromFilesAsync);
        AddFromFolderCommand = new AsyncRelayCommand(AddFromFolderAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private TempFile? _tempThumbFile;
    private TempDirectory? _tempDir;

    #endregion

    #region Commands

    public ICommand EditGameTargetsCommand { get; }
    public ICommand ImportPatchCommand { get; }
    public ICommand BrowseThumbnailCommand { get; }
    public ICommand RemoveThumbnailCommand { get; }
    public ICommand AddFileCommand { get; }
    public ICommand AddFromFilesCommand { get; }
    public ICommand AddFromFolderCommand { get; }

    #endregion

    #region Public Properties

    // Metadata
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    public string Website { get; set; } = String.Empty;
    public int Version_Major { get; set; } = 1;
    public int Version_Minor { get; set; }
    public int Version_Revision { get; set; }
    public string ID { get; set; }
    public GameInstallation[] GameTargets { get; private set; }
    public ObservableCollection<string> GameTargetNames { get; private set; }
    public BitmapSource? Thumbnail { get; set; }

    // Files
    public ObservableCollection<FileViewModel> Files { get; } = new();
    public FileViewModel? SelectedFile { get; set; }

    // Locations
    public ObservableCollection<AvailableFileLocation> AvailableLocations { get; }
    public AvailableFileLocation SelectedLocation { get; set; }

    // Other
    public LoaderViewModel LoaderViewModel { get; }

    #endregion

    #region Private Methods

    [MemberNotNull(nameof(GameTargets))]
    [MemberNotNull(nameof(SelectedLocation))]
    [MemberNotNull(nameof(GameTargetNames))]
    private void SetGameTargets(IEnumerable<GameInstallation> gameTargets)
    {
        // Set the game targets
        GameTargets = gameTargets.ToArray();
        GameTargetNames = new ObservableCollection<string>(
            GameTargets.Select(x => x.GameDescriptor).Distinct().Select(x => x.GameDescriptorName));

        // Clear previous locations
        AvailableLocations.Clear();

        // Always add the physical game location first
        AvailableLocations.Add(new AvailableFileLocation(new ResourceLocString(nameof(Resources.Patcher_PhysicalGameLocation)), String.Empty, String.Empty));

        // Add additional locations based on the game targets
        foreach (GameInstallation gameInstallation in GameTargets)
        {
            string? archiveID = gameInstallation.GameDescriptor.GetArchiveDataManager(null)?.ID;

            if (archiveID == null)
                continue;

            foreach (string archivePath in gameInstallation.GameDescriptor.GetArchiveFilePaths(null))
            {
                AvailableFileLocation? existingLocation = AvailableLocations.
                    FirstOrDefault(x => x.Location == archivePath && x.LocationID == archiveID);

                if (existingLocation != null)
                    existingLocation.GameTargets.Add(gameInstallation);
                else
                    AvailableLocations.Add(new AvailableFileLocation(archivePath, archivePath, archiveID));
            }
        }

        // NOTE: I commented this out for now since we might not need it. The importing keeps
        //       files even if they're in a location which isn't listed. We should probably
        //       look into it a bit more, but it should be fine like this.
        // Remove files which use removed locations
        /*
        foreach (FileViewModel file in Files.ToList())
        {
            if (AvailableLocations.All(x => x.LocationID != file.LocationID))
                Files.Remove(file);
        }*/

        // Default to select the first location
        SelectedLocation = AvailableLocations.First();
    }

    private void AddFile(FileViewModel file)
    {
        string normalizedPath = file.FilePath.ToLowerInvariant().Replace('/', '\\');

        // Don't allow adding files which conflict with existing locations
        if (file.Location == String.Empty && AvailableLocations.Any(x => x.Location.Replace('/', '\\').ToLowerInvariant() == normalizedPath))
            return;

        Files.Add(file);
    }

    #endregion

    #region Public Methods

    public async Task EditGameTargetsAsync()
    {
        var availableGameInstallations = Services.Games.GetInstalledGames().Where(x => x.GameDescriptor.AllowPatching);
        GamesSelectionResult result = await Services.UI.SelectGamesAsync(new GamesSelectionViewModel(availableGameInstallations, GameTargets)
        {
            // TODO-UPDATE: Localize
            Title = "Select game targets",
            MultiSelection = true,
        });

        if (result.CanceledByUser)
            return;

        // Set the game targets to the new selection
        SetGameTargets(result.SelectedGames);
    }

    public async Task ImportPatchAsync()
    {
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = Resources.PatchCreator_SelectImportPatch,
            ExtensionFilter = new FileFilterItem($"*{PatchFile.FileExtension}", Resources.Patcher_FileType).StringRepresentation,
        });

        if (browseResult.CanceledByUser)
            return;

        FileSystemPath patchFilePath = browseResult.SelectedFile;
        Logger.Trace("Importing from patch at {0}", patchFilePath);

        try
        {
            using (LoadState state = await LoaderViewModel.RunAsync(Resources.PatchCreator_ImportingPatch_Status, canCancel: true))
            {
                // Create a context
                using RCPContext context = new(String.Empty);

                PatchFile patchFile;

                try
                {
                    // Read the patch file
                    patchFile = context.ReadRequiredFileData<PatchFile>(patchFilePath, removeFileWhenComplete: false);
                }
                catch (UnsupportedFormatVersionException ex)
                {
                    Logger.Warn(ex, "Importing patch to creator");

                    await Services.MessageUI.DisplayMessageAsync(Resources.Patcher_ReadPatchNewerVersionError,
                        MessageType.Error);
                    return;
                }

                PatchMetadata metadata = patchFile.Metadata;

                // Copy metadata
                Name = metadata.Name;
                Description = metadata.Description;
                Author = metadata.Author;
                Website = metadata.Website;
                Version_Major = metadata.Version.Major;
                Version_Minor = metadata.Version.Minor;
                Version_Revision = metadata.Version.Revision;
                ID = metadata.ID;

                // Extract thumbnail
                if (patchFile.HasThumbnail)
                {
                    // Dispose the temp file for the thumbnail if it existed before
                    _tempThumbFile?.Dispose();

                    _tempThumbFile = new TempFile(false);

                    using (Stream thumbStream = patchFile.ThumbnailResource.ReadData(context, true))
                    {
                        using FileStream dstStream = File.Create(_tempThumbFile.TempPath);
                        await thumbStream.CopyToAsync(dstStream);
                    }

                    BitmapImage thumb = new();
                    thumb.BeginInit();
                    thumb.CacheOption = BitmapCacheOption.OnLoad; // Required to allow the temp file to be deleted
                    thumb.UriSource = new Uri(_tempThumbFile.TempPath);
                    thumb.EndInit();

                    if (thumb.CanFreeze)
                        thumb.Freeze();

                    Thumbnail = thumb;
                }

                _tempDir ??= new TempDirectory(true);

                // Add added files and extract resources to temp
                for (int i = 0; i < patchFile.AddedFiles.Length; i++)
                {
                    state.CancellationToken.ThrowIfCancellationRequested();

                    PatchFilePath filePath = patchFile.AddedFiles[i];
                    PackagedResourceEntry resource = patchFile.AddedFileResources[i];
                    PackagedResourceChecksum checksum = patchFile.AddedFileChecksums[i];

                    state.SetProgress(new Progress(i, patchFile.AddedFiles.Length));

                    FileSystemPath tempFilePath = _tempDir.TempPath + metadata.ID + filePath.FullFilePath;

                    Directory.CreateDirectory(tempFilePath.Parent);

                    using Stream srcStream = resource.ReadData(context, true);
                    using FileStream dstStream = File.Create(tempFilePath);

                    await srcStream.CopyToAsync(dstStream);

                    Files.Add(new FileViewModel(
                        sourceFilePath: tempFilePath,
                        filePath: filePath.FilePath,
                        location: filePath.Location,
                        locationId: filePath.LocationID,
                        checksum: checksum));
                }

                state.SetProgress(new Progress(patchFile.AddedFiles.Length, patchFile.AddedFiles.Length));

                // Add removed files
                foreach (PatchFilePath filePath in patchFile.RemovedFiles)
                {
                    Files.Add(new FileViewModel(
                        sourceFilePath: FileSystemPath.EmptyPath,
                        filePath: filePath.FilePath,
                        location: filePath.Location,
                        locationId: filePath.LocationID));
                }

                Logger.Info("Imported patch {0} with format version {1}", metadata.Name, patchFile.FormatVersion);
            }
        }
        catch (OperationCanceledException ex)
        {
            Logger.Trace(ex, "Cancelled importing patch to creator");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Importing patch to creator");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ReadPatchGenericError);
        }
    }

    public async Task BrowseThumbnailAsync()
    {
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = Resources.PatchCreator_SelectThumbnailHeader,
            ExtensionFilter = new FileFilterItemCollection(new FileFilterItem[]
            {
                new("*.png", String.Empty),
                new("*.jpg", String.Empty),
                new("*.jpeg", String.Empty),
            }).CombineAll(Resources.FileSelection_ImageFormat).StringRepresentation,
        });

        if (browseResult.CanceledByUser)
            return;

        Thumbnail = new BitmapImage(new Uri(browseResult.SelectedFile));

        if (Thumbnail.CanFreeze)
            Thumbnail.Freeze();

        Logger.Info("Changed the thumbnail");
    }

    public void RemoveThumbnail()
    {
        Thumbnail = null;
        Logger.Info("Removed the thumbnail");
    }

    public void AddFile()
    {
        FileViewModel vm = new(FileSystemPath.EmptyPath, String.Empty, SelectedLocation.Location,
            SelectedLocation.LocationID);
        Files.Add(vm);
        vm.IsSelected = true;
    }

    public async Task AddFromFilesAsync()
    {
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            Title = Resources.PatchCreator_AddFromFilesHeader,
            MultiSelection = true,
        });

        if (browseResult.CanceledByUser)
            return;

        Logger.Info("Adding files");

        // Clear selection
        SelectedFile = null;

        foreach (FileSystemPath file in browseResult.SelectedFiles)
        {
            AddFile(new FileViewModel(
                sourceFilePath: file,
                filePath: file.Name,
                location: SelectedLocation.Location,
                locationId: SelectedLocation.LocationID));
        }
    }

    public async Task AddFromFolderAsync()
    {
        DirectoryBrowserResult browseResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
        {
            Title = Resources.PatchCreator_AddFromFolderHeader,
        });

        if (browseResult.CanceledByUser)
            return;

        Logger.Info("Adding files from folder");

        // Clear selection
        SelectedFile = null;

        try
        {
            foreach (FileSystemPath file in Directory.EnumerateFiles(browseResult.SelectedDirectory, "*", SearchOption.AllDirectories))
            {
                AddFile(new FileViewModel(
                    sourceFilePath: file,
                    filePath: file - browseResult.SelectedDirectory,
                    location: SelectedLocation.Location,
                    locationId: SelectedLocation.LocationID));
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Adding files from folder");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.PatchCreator_AddFromFolderError);
        }
    }

    public async Task<bool> CreatePatchAsync()
    {
        using (LoadState state = await LoaderViewModel.RunAsync(Resources.PatchCreator_Create_Status, canCancel: true))
        {
            PatchVersion version = new(Version_Major, Version_Minor, Version_Revision);

            Logger.Info("Creating the patch '{0}' with version {1} and ID {2}", Name, version, ID);

            SaveFileResult browseResult = await Services.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                Title = Resources.PatchCreator_CreateSaveFileHeader,
                Extensions = new FileFilterItem($"*{PatchFile.FileExtension}", Resources.Patcher_FileType).StringRepresentation,
                DefaultName = Name
            });

            if (browseResult.CanceledByUser)
                return false;

            try
            {
                await Task.Run(() =>
                {
                    // Create a context and add the patch file
                    using RCPContext context = new(String.Empty);
                    LinearFile binaryFile = context.AddFile(new LinearFile(context, browseResult.SelectedFileLocation));

                    // Create a new patch file
                    PatchFile patchFile = new()
                    {
                        FormatVersion = PatchFile.LatestFormatVersion,
                        Metadata = new PatchMetadata
                        {
                            ID = ID,
                            GameIds = GameTargets.Select(x => x.GameId).ToArray(),
                            Name = Name.IsNullOrWhiteSpace() ? "Unnamed patch" : Name,
                            Description = Description,
                            Author = Author,
                            Website = Website,
                            Version = version,
                            ModifiedDate = DateTime.Now
                        },
                    };

                    // Initialize the file
                    patchFile.Init(binaryFile.StartPointer);

                    List<(PatchFilePath FilePath, PackagedResourceChecksum Checksum, PackagedResourceEntry Resource)>
                        addedFiles = new();
                    List<PatchFilePath> removedFiles = new();
                    long totalSize = 0;

                    Stopwatch sw = Stopwatch.StartNew();

                    int fileIndex = 0;

                    // Add the file entries and calculate the checksums
                    foreach (FileViewModel file in Files)
                    {
                        state.CancellationToken.ThrowIfCancellationRequested();
                        state.SetProgress(new Progress((double)fileIndex / Files.Count, 2));
                        fileIndex++;

                        if (file.FilePath.IsNullOrWhiteSpace())
                            continue;

                        if (file.IsFileAdded)
                        {
                            long length;
                            PackagedResourceChecksum checksum;

                            if (file.Checksum == null)
                            {
                                using FileStream stream = File.OpenRead(file.SourceFilePath);

                                // Calculate the checksum
                                checksum = file.Checksum ?? PackagedResourceChecksum.FromStream(stream);

                                length = stream.Length;
                            }
                            else
                            {
                                checksum = file.Checksum;
                                length = file.SourceFilePath.GetFileInfo().Length;
                            }

                            PackagedResourceEntry resource = new();
                            addedFiles.Add((file.PatchFilePath, checksum, resource));
                            resource.SetPendingImport(() => File.OpenRead(file.SourceFilePath), false);

                            // Update the total size
                            totalSize += length;
                        }
                        else
                        {
                            // Add to the patch
                            removedFiles.Add(file.PatchFilePath);
                        }
                    }

                    state.SetProgress(new Progress((double)fileIndex / Files.Count, 2));

                    sw.Stop();
                    Logger.Debug("Calculated file checksums in {0} ms", sw.ElapsedMilliseconds);

                    // Add the thumbnail if there is one
                    if (Thumbnail != null)
                    {
                        Logger.Info("Adding patch thumbnail");

                        patchFile.ThumbnailResource = new PackagedResourceEntry();
                        patchFile.ThumbnailResource.SetPendingImport(() =>
                        {
                            PngBitmapEncoder encoder = new();
                            encoder.Frames.Add(BitmapFrame.Create(Thumbnail));

                            MemoryStream memStream = new();
                            encoder.Save(memStream);
                            memStream.Position = 0;

                            return memStream;
                        }, false);
                        patchFile.HasThumbnail = true;

                        Logger.Info("Added patch thumbnail");
                    }

                    patchFile.AddedFiles = addedFiles.Select(x => x.FilePath).ToArray();
                    patchFile.AddedFileChecksums = addedFiles.Select(x => x.Checksum).ToArray();
                    patchFile.AddedFileResources = addedFiles.Select(x => x.Resource).ToArray();
                    patchFile.RemovedFiles = removedFiles.ToArray();
                    patchFile.Metadata.TotalSize = totalSize;

                    // Pack the file
                    // ReSharper disable once AccessToDisposedClosure
                    patchFile.WriteAndPackResources(x => state.SetProgress(new Progress(1 + x.Percentage_100 / 100, 2)),
                        state.CancellationToken);

                    // Dispose temporary files
                    _tempDir?.Dispose();
                    _tempDir = null;
                    _tempThumbFile?.Dispose();
                    _tempThumbFile = null;
                });

                Logger.Info("Created patch");

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.PatchCreator_CreateSuccess);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                Logger.Trace(ex, "Cancelled creating patch");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Creating patch");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.PatchCreator_CreateError);

                return false;
            }
        }
    }

    public void Dispose()
    {
        _tempDir?.Dispose();
        _tempDir = null;

        _tempThumbFile?.Dispose();
        _tempThumbFile = null;
    }

    #endregion

    #region Data Types

    public class FileViewModel : BaseViewModel
    {
        public FileViewModel(
            FileSystemPath sourceFilePath, 
            string filePath, 
            string location, 
            string locationId,
            PackagedResourceChecksum? checksum = null)
        {
            SourceFilePath = sourceFilePath;
            FilePath = filePath;
            LocationDisplayName = location == String.Empty 
                ? new ResourceLocString(nameof(Resources.Patcher_PhysicalGameLocation)) 
                : location;
            Location = location;
            LocationID = locationId;
            Checksum = checksum;
        }

        public string Location { get; }
        public LocalizedString LocationDisplayName { get; }
        public string LocationID { get; }
        public PackagedResourceChecksum? Checksum { get; }

        public FileSystemPath SourceFilePath { get; set; }
        public string FilePath { get; set; }

        public bool IsSelected { get; set; }

        public bool IsFileAdded => SourceFilePath.FileExists;
        public bool IsImported => Checksum != null;
        public PatchFilePath PatchFilePath => new(Location, LocationID, FilePath);
    }

    public record AvailableFileLocation(LocalizedString DisplayName, string Location, string LocationID)
    {
        public List<GameInstallation> GameTargets { get; } = new();
    }

    #endregion
}
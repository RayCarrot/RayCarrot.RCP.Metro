using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BinarySerializer;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchCreatorViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public PatchCreatorViewModel(Games game)
    {
        Game = game;
        ID = PatchFile.GenerateID();
        AvailableLocations = new ObservableCollection<AvailableFileLocation>()
        {
            // TODO-UPDATE: Localize
            new("Game", String.Empty, String.Empty)
        };

        FileSystemPath installDir = game.GetInstallDir();
        string? archiveID = game.GetGameInfo().GetArchiveDataManager?.ID;
        IEnumerable<AvailableFileLocation>? archiveLocations = archiveID == null ? null : game.GetGameInfo().
            GetArchiveFilePaths(installDir)?.
            Where(x => x.FileExists).
            Select(x => x - installDir).
            Select(x => new AvailableFileLocation(x.FullPath, x.FullPath, archiveID));

        if (archiveLocations != null)
            AvailableLocations.AddRange(archiveLocations);

        SelectedLocation = AvailableLocations.First();

        LoadOperation = new BindableOperation();

        BrowseThumbnailCommand = new AsyncRelayCommand(BrowseThumbnailAsync);
        RemoveThumbnailCommand = new RelayCommand(RemoveThumbnail);
        AddFilesCommand = new AsyncRelayCommand(AddFilesAsync);
        AddFilesFromFolderCommand = new AsyncRelayCommand(AddFilesFromFolderAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private TempDirectory? _tempDir;

    #endregion

    #region Commands

    public ICommand BrowseThumbnailCommand { get; }
    public ICommand RemoveThumbnailCommand { get; }
    public ICommand AddFilesCommand { get; }
    public ICommand AddFilesFromFolderCommand { get; }

    #endregion

    #region Public Properties

    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    public int Revision { get; set; }
    public string ID { get; set; }
    public Games Game { get; }
    public BitmapSource? Thumbnail { get; set; }

    public ObservableCollection<FileViewModel> Files { get; } = new();
    public FileViewModel? SelectedFile { get; set; }

    public ObservableCollection<AvailableFileLocation> AvailableLocations { get; }
    public AvailableFileLocation SelectedLocation { get; set; }

    public bool IsImported { get; set; }

    public BindableOperation LoadOperation { get; }

    #endregion

    #region Public Methods

    public async Task<bool> ImportFromPatchAsync(FileSystemPath patchFilePath)
    {
        // TODO-UPDATE: Localize
        using (DisposableOperation operation = await LoadOperation.RunAsync("Importing from existing patch"))
        {
            Logger.Trace("Importing from patch at {0}", patchFilePath);

            try
            {
                // Create a context
                using RCPContext context = new(String.Empty);
                context.AddFile(new LinearFile(context, patchFilePath));

                PatchFile patchFile;

                try
                {
                    // Read the patch file
                    patchFile = FileFactory.Read<PatchFile>(context, patchFilePath);
                }
                catch (UnsupportedFormatVersionException ex)
                {
                    Logger.Warn(ex, "Importing patch to creator");

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync("The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);

                    return false;
                }

                PatchMetadata metadata = patchFile.Metadata;

                // Copy metadata
                Name = metadata.Name;
                Description = metadata.Description;
                Author = metadata.Author;
                Revision = metadata.Revision + 1;
                ID = metadata.ID;

                // Extract thumbnail
                if (patchFile.HasThumbnail)
                {
                    using MemoryStream thumbStream = new(patchFile.Thumbnail);

                    Thumbnail = new PngBitmapDecoder(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.FirstOrDefault();

                    if (Thumbnail?.CanFreeze == true)
                        Thumbnail.Freeze();
                }

                _tempDir = new TempDirectory(true);

                // Extract resources to temp
                int fileIndex = 0;
                foreach (PatchFileResourceEntry resource in patchFile.AddedFiles)
                {
                    operation.SetProgress(new Progress(fileIndex, patchFile.AddedFiles.Length));
                    fileIndex++;

                    FileSystemPath tempFilePath = _tempDir.TempPath + resource.FilePath.FullFilePath;

                    Directory.CreateDirectory(tempFilePath.Parent);

                    using Stream srcStream = resource.ReadData(context.Deserializer, patchFile.Offset);
                    using FileStream dstStream = File.Create(tempFilePath);

                    await srcStream.CopyToAsync(dstStream);

                    Files.Add(new FileViewModel()
                    {
                        SourceFilePath = tempFilePath,
                        Location = resource.FilePath.Location,
                        LocationID = resource.FilePath.LocationID,
                        FilePath = resource.FilePath.FilePath,
                        Checksum = resource.Checksum,
                    });
                }

                operation.SetProgress(new Progress(fileIndex, patchFile.AddedFiles.Length));

                IsImported = true;

                Logger.Info("Imported patch {0} with version {1}", metadata.Name, patchFile.Version);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Importing patch to creator");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "The selected patch could not be read");

                return false;
            }
        }
    }

    public async Task BrowseThumbnailAsync()
    {
        // TODO-UPDATE: Localize
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = "Select a thumbnail",
            ExtensionFilter = new FileFilterItemCollection(new FileFilterItem[]
            {
                new("*.png", String.Empty),
                new("*.jpg", String.Empty),
                new("*.jpeg", String.Empty),
            }).CombineAll("Image").StringRepresentation,
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

    public async Task AddFilesAsync()
    {
        // TODO-UPDATE: Localize
        FileBrowserResult browseResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            Title = "Select files to add",
            MultiSelection = true,
        });

        if (browseResult.CanceledByUser)
            return;

        Logger.Info("Adding files");

        // Clear selection
        SelectedFile = null;

        foreach (FileSystemPath file in browseResult.SelectedFiles)
        {
            Files.Add(new FileViewModel()
            {
                SourceFilePath = file,
                Location = SelectedLocation.Location,
                LocationID = SelectedLocation.LocationID,
                FilePath = file.Name,
            });
        }
    }

    public async Task AddFilesFromFolderAsync()
    {
        // TODO-UPDATE: Localize
        DirectoryBrowserResult browseResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
        {
            Title = "Select folder to add",
        });

        if (browseResult.CanceledByUser)
            return;

        Logger.Info("Adding files from folder");

        // Clear selection
        SelectedFile = null;

        try
        {
            // TODO-UPDATE: Localize
            // TODO-UPDATE: Keep async?
            foreach (FileSystemPath file in Directory.EnumerateFiles(browseResult.SelectedDirectory, "*", SearchOption.AllDirectories))
            {
                Files.Add(new FileViewModel()
                {
                    SourceFilePath = file,
                    Location = SelectedLocation.Location,
                    LocationID = SelectedLocation.LocationID,
                    FilePath = file - browseResult.SelectedDirectory,
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Adding files from folder");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when adding the selected files");
        }
    }

    public async Task<bool> CreatePatchAsync()
    {
        // TODO-UPDATE: Localize
        using (await LoadOperation.RunAsync("Creating patch"))
        {
            Logger.Info("Creating the patch '{0}' with revision {1} and ID {2}", Name, Revision, ID);

            // TODO-UPDATE: Localize
            SaveFileResult browseResult = await Services.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                Title = "Save patch file",
                Extensions = new FileFilterItem($"*{PatchFile.FileExtension}", "Game Patch").StringRepresentation,
            });

            if (browseResult.CanceledByUser)
                return false;

            try
            {
                await Task.Run(() =>
                {
                    // Create a context and add the patch file
                    using RCPContext context = new(String.Empty);
                    LinearFile binaryFile = context.AddFile(new LinearFile(context, browseResult.SelectedFileLocation)
                    {
                        RecreateOnWrite = false
                    });

                    // Create a new patch file
                    PatchFile patchFile = new()
                    {
                        Version = PatchFile.LatestVersion,
                        Metadata = new PatchMetadata
                        {
                            ID = ID,
                            Game = Game,
                            Name = Name,
                            Description = Description,
                            Author = Author,
                            Revision = Revision,
                            ModifiedDate = DateTime.Now
                        },
                    };

                    Dictionary<FileViewModel, PatchFileResourceEntry> addedFiles = new();
                    List<PatchFilePath> removedFiles = new();
                    long totalSize = 0;

                    // Add the file entries
                    foreach (FileViewModel file in Files.Where(x => x.IsValid))
                    {
                        if (file.IsFileAdded)
                        {
                            using FileStream stream = File.OpenRead(file.SourceFilePath);

                            // Calculate the checksum
                            byte[] checksum = file.Checksum ?? PatchFile.CalculateChecksum(stream);
                            
                            addedFiles.Add(file, new PatchFileResourceEntry
                            {
                                FilePath = file.PatchFilePath,
                                Checksum = checksum,

                                // Gets set when we pack the resources
                                DataOffset = 0,
                                CompressedDataLength = 0,
                                DecompressedDataLength = 0
                            });

                            // Update the total size
                            totalSize += stream.Length;
                        }
                        else
                        {
                            // Add to the manifest
                            removedFiles.Add(file.PatchFilePath);
                        }
                    }

                    // Add the thumbnail if there is one
                    if (Thumbnail != null)
                    {
                        Logger.Info("Adding patch thumbnail");

                        PngBitmapEncoder encoder = new();
                        encoder.Frames.Add(BitmapFrame.Create(Thumbnail));

                        using MemoryStream memStream = new();
                        encoder.Save(memStream);
                        memStream.Position = 0;

                        patchFile.Thumbnail = memStream.ToArray();
                        patchFile.HasThumbnail = true;

                        Logger.Info("Added patch thumbnail");
                    }

                    patchFile.AddedFiles = addedFiles.Values.ToArray();
                    patchFile.RemovedFiles = removedFiles.ToArray();
                    patchFile.Metadata.TotalSize = totalSize;

                    // Calculate the header size to get the start offset for packing the resources
                    patchFile.Init(binaryFile.StartPointer);
                    patchFile.RecalculateSize();
                    long dataOffset = patchFile.Size;

                    // Create the patch files and pack the files. We leave the header empty for now.
                    using (Stream patchFileStream = File.Create(browseResult.SelectedFileLocation))
                    {
                        patchFileStream.Position = dataOffset;

                        foreach (FileViewModel file in Files.Where(x => x.IsValid && x.IsFileAdded))
                        {
                            // Get the resource entry
                            PatchFileResourceEntry resourceEntry = addedFiles[file];

                            // Open the file to pack
                            using FileStream stream = File.OpenRead(file.SourceFilePath);

                            // Compress the file
                            using (DeflateStream deflateStream = new(patchFileStream, CompressionLevel.Fastest, leaveOpen: true))
                                // Write to the patch file
                                stream.CopyTo(deflateStream);

                            // Get the compressed data length
                            long compressedLength = patchFileStream.Position - dataOffset;

                            // Set the resource entry values
                            resourceEntry.DataOffset = dataOffset;
                            resourceEntry.CompressedDataLength = compressedLength;
                            resourceEntry.DecompressedDataLength = stream.Length;

                            // Update the data offset for the next resource
                            dataOffset += compressedLength;
                        }
                    }

                    // Write the patch file header
                    FileFactory.Write(context, binaryFile.FilePath, patchFile);
                    
                    // Dispose temporary files
                    _tempDir?.Dispose();
                    _tempDir = null;
                });

                Logger.Info("Created patch");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync("The patch was saved successfully", MessageType.Success);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Creating patch");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when creating the patch");

                return false;
            }
        }
    }

    public void Dispose()
    {
        _tempDir?.Dispose();
        _tempDir = null;
    }

    #endregion

    #region Data Types

    public class FileViewModel : BaseViewModel
    {
        public FileSystemPath SourceFilePath { get; set; }
        public string Location { get; set; } = String.Empty;
        public string LocationID { get; set; } = String.Empty;
        public string FilePath { get; set; } = String.Empty;

        public byte[]? Checksum { get; set; }

        public bool IsSelected { get; set; }

        public bool IsValid => !FilePath.IsNullOrWhiteSpace();
        public bool IsFileAdded => SourceFilePath.FileExists;
        public bool IsImported => Checksum != null;
        public PatchFilePath PatchFilePath => new(Location, LocationID, FilePath);
    }

    public record AvailableFileLocation(LocalizedString DisplayName, string Location, string LocationID);

    #endregion
}
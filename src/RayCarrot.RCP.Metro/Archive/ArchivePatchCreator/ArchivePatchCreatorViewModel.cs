using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RayCarrot.RCP.Metro.Archive;

public class ArchivePatchCreatorViewModel : BaseViewModel, IDisposable
{
    public ArchivePatchCreatorViewModel()
    {
        ID = PatchFile.GenerateID();

        BrowseThumbnailCommand = new AsyncRelayCommand(BrowseThumbnailAsync);
        RemoveThumbnailCommand = new RelayCommand(RemoveThumbnail);
        AddFileCommand = new RelayCommand(AddFile);
        AddFileFromFolderCommand = new AsyncRelayCommand(AddFileFromFolderAsync);
    }

    private TempDirectory? _tempDir;

    public ICommand BrowseThumbnailCommand { get; }
    public ICommand RemoveThumbnailCommand { get; }
    public ICommand AddFileCommand { get; }
    public ICommand AddFileFromFolderCommand { get; }

    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    public int Revision { get; set; }
    public string ID { get; set; }
    public BitmapSource? Thumbnail { get; set; }

    public ObservableCollection<FileViewModel> Files { get; } = new();
    public FileViewModel? SelectedFile { get; set; }

    public bool IsImported { get; set; }

    // TODO-UPDATE: Use bindable operation
    public string? LoadingMessage { get; set; }
    public bool IsLoading { get; set; }
    public double CurrentProgress { get; set; }
    public double MinProgress { get; set; }
    public double MaxProgress { get; set; }
    public bool HasProgress { get; set; }

    public async Task<bool> ImportFromPatchAsync(FileSystemPath patchFilePath)
    {
        if (IsLoading)
            return false;

        // TODO-UPDATE: Localize
        LoadingMessage = "Importing from existing patch";
        HasProgress = false;
        IsLoading = true;

        // TODO-UPDATE: Try/catch

        try
        {
            using PatchFile patchFile = new(patchFilePath, true);

            PatchManifest manifest = patchFile.ReadManifest();

            if (manifest.PatchVersion > PatchFile.Version)
            {
                await Services.MessageUI.DisplayMessageAsync("The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);

                return false;
            }

            Name = manifest.Name ?? String.Empty;
            Description = manifest.Description ?? String.Empty;
            Author = manifest.Author ?? String.Empty;
            Revision = manifest.Revision + 1;
            ID = manifest.ID;

            if (manifest.HasAsset(PatchAsset.Thumbnail))
            {
                using Stream thumbStream = patchFile.GetPatchAsset(PatchAsset.Thumbnail);

                Thumbnail = new PngBitmapDecoder(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.FirstOrDefault();

                if (Thumbnail?.CanFreeze == true)
                    Thumbnail.Freeze();
            }

            _tempDir = new TempDirectory(true);

            // Extract resources to temp
            if (manifest.AddedFiles != null && manifest.AddedFileChecksums != null)
            {
                CurrentProgress = 0;
                MinProgress = 0;
                MaxProgress = manifest.AddedFiles.Length;
                HasProgress = true;

                for (var i = 0; i < manifest.AddedFiles.Length; i++)
                {
                    string addedFile = manifest.AddedFiles[i];
                    string checksum = manifest.AddedFileChecksums[i];

                    FileSystemPath tempFilePath = _tempDir.TempPath + addedFile;

                    Directory.CreateDirectory(tempFilePath.Parent);

                    using Stream file = patchFile.GetPatchResource(addedFile, false);
                    using Stream tempFileStream = File.Create(tempFilePath);
                    await file.CopyToAsync(tempFileStream);

                    Files.Add(new FileViewModel()
                    {
                        SourceFilePath = tempFilePath,
                        ArchiveFilePath = addedFile,
                        Checksum = checksum,
                    });

                    CurrentProgress++;
                }
            }

            IsImported = true;

            return true;
        }
        finally
        {
            IsLoading = false;
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
    }

    public void RemoveThumbnail() => Thumbnail = null;

    public void AddFile()
    {
        // Only add a new entry if the previous one is valid
        if (Files.Count > 0 && !Files.Last().IsValid)
        {
            SelectedFile = Files.Last();
            return;
        }

        FileViewModel file = new();
        Files.Add(file);
        SelectedFile = file;
    }

    public async Task AddFileFromFolderAsync()
    {
        // TODO-UPDATE: Localize
        DirectoryBrowserResult browseResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
        {
            Title = "Select folder to add",
        });

        if (browseResult.CanceledByUser)
            return;

        // If last file is invalid we remove it
        if (Files.Count > 0 && !Files.Last().IsValid)
            Files.RemoveAt(Files.Count - 1);

        // Clear selection
        SelectedFile = null;

        // TODO-UPDATE: Try/catch
        foreach (FileSystemPath file in Directory.EnumerateFiles(browseResult.SelectedDirectory, "*", SearchOption.AllDirectories))
        {
            Files.Add(new FileViewModel()
            {
                SourceFilePath = file,
                ArchiveFilePath = file - browseResult.SelectedDirectory,
            });
        }
    }

    public async Task<bool> CreatePatchAsync()
    {
        if (IsLoading)
            return false;

        // TODO-UPDATE: Localize
        LoadingMessage = "Creating patch";
        HasProgress = false;
        IsLoading = true;

        try
        {
            // TODO-UPDATE: Localize
            SaveFileResult browseResult = await Services.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                Title = "Save patch file",
                Extensions = new FileFilterItem("*.ap", "Archive Patch").StringRepresentation,
            });

            if (browseResult.CanceledByUser)
                return false;

            if (browseResult.SelectedFileLocation.FileExists)
                browseResult.SelectedFileLocation.DeleteFile();

            // TODO-UPDATE: Try/catch

            await Task.Run(() =>
            {
                using PatchFile patchFile = new(browseResult.SelectedFileLocation);

                List<string> addedFiles = new();
                List<string> addedFileChecksums = new();
                List<string> removedFiles = new();
                List<string> assets = new();
                long totalSize = 0;

                foreach (FileViewModel file in Files.Where(x => x.IsValid))
                {
                    if (file.IsFileAdded)
                    {
                        using FileStream stream = File.OpenRead(file.SourceFilePath);

                        // Calculate the checksum
                        string checksum = file.Checksum ?? PatchFile.CalculateChecksum(stream);
                        stream.Position = 0;

                        // Add the file
                        patchFile.AddPatchResource(file.ArchiveFilePath, false, stream);

                        // Add to the manifest
                        addedFiles.Add(file.ArchiveFilePath);
                        addedFileChecksums.Add(checksum);

                        // Update the total size
                        totalSize += stream.Length;
                    }
                    else
                    {
                        // Add to the manifest
                        removedFiles.Add(file.ArchiveFilePath);
                    }
                }

                // Add the thumbnail if there is one
                if (Thumbnail != null)
                {
                    PngBitmapEncoder encoder = new();
                    encoder.Frames.Add(BitmapFrame.Create(Thumbnail));

                    using MemoryStream memStream = new();
                    encoder.Save(memStream);
                    memStream.Position = 0;

                    // Add the asset
                    patchFile.AddPatchAsset(PatchAsset.Thumbnail, memStream);

                    // Add to the manifest
                    assets.Add(PatchAsset.Thumbnail);
                }

                // Write the manifest
                patchFile.WriteManifest(new PatchManifest(
                    ID: ID,
                    PatchVersion: PatchFile.Version,
                    Name: Name,
                    Description: Description,
                    Author: Author,
                    TotalSize: totalSize,
                    ModifiedDate: DateTime.Now,
                    Revision: Revision,
                    AddedFiles: addedFiles.ToArray(),
                    AddedFileChecksums: addedFileChecksums.ToArray(),
                    RemovedFiles: removedFiles.ToArray(),
                    Assets: assets.ToArray()));

                patchFile.Apply();

                // Dispose temporary files
                _tempDir?.Dispose();
                _tempDir = null;
            });

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync("The patch was saved successfully", MessageType.Success);

            return true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        _tempDir?.Dispose();
        _tempDir = null;
    }

    public class FileViewModel : BaseViewModel
    {
        public FileSystemPath SourceFilePath { get; set; }
        public string ArchiveFilePath { get; set; } = String.Empty;

        public string? Checksum { get; set; }

        public bool IsSelected { get; set; }

        public bool IsValid => !ArchiveFilePath.IsNullOrWhiteSpace();
        public bool IsFileAdded => SourceFilePath.FileExists;
        public bool IsImported => Checksum != null;
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RayCarrot.RCP.Metro.Archive;

public class ArchivePatchCreatorViewModel : BaseViewModel
{
    public ArchivePatchCreatorViewModel()
    {
        BrowseThumbnailCommand = new AsyncRelayCommand(BrowseThumbnailAsync);
        RemoveThumbnailCommand = new RelayCommand(RemoveThumbnail);
        AddFileCommand = new RelayCommand(AddFile);
        AddFileFromFolderCommand = new AsyncRelayCommand(AddFileFromFolderAsync);
    }

    public ICommand BrowseThumbnailCommand { get; }
    public ICommand RemoveThumbnailCommand { get; }
    public ICommand AddFileCommand { get; }
    public ICommand AddFileFromFolderCommand { get; }

    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    public int Revision { get; set; }
    public BitmapImage? Thumbnail { get; set; }

    public ObservableCollection<FileViewModel> Files { get; } = new();
    public FileViewModel? SelectedFile { get; set; }

    public bool IsLoading { get; set; }

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
                using Patch patch = new(browseResult.SelectedFileLocation);

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
                        string checksum = Patch.CalculateChecksum(stream);
                        stream.Position = 0;

                        // Add the file
                        patch.AddPatchResource(file.ArchiveFilePath, false, stream);

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
                    patch.AddPatchAsset(PatchAsset.Thumbnail, memStream);

                    // Add to the manifest
                    assets.Add(PatchAsset.Thumbnail);
                }

                // Write the manifest
                patch.WriteManifest(new PatchManifest(
                    id: Patch.GenerateID(), // TODO-UPDATE: Should we allow existing patches to be updated, retaining their ID?
                    containerVersion: PatchContainer.Version,
                    name: Name,
                    description: Description,
                    author: Author,
                    flags: PatchFlags.None,
                    totalSize: totalSize,
                    modifiedDate: DateTime.Now,
                    revision: Revision,
                    addedFiles: addedFiles.ToArray(),
                    addedFileChecksums: addedFileChecksums.ToArray(),
                    removedFiles: removedFiles.ToArray(),
                    assets: assets.ToArray()));

                patch.Apply();
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

    public class FileViewModel : BaseViewModel
    {
        public FileSystemPath SourceFilePath { get; set; }
        public string ArchiveFilePath { get; set; } = String.Empty;

        public bool IsSelected { get; set; }

        public bool IsValid => !ArchiveFilePath.IsNullOrWhiteSpace();
        public bool IsFileAdded => SourceFilePath.FileExists;
    }
}
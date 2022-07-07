﻿using System;
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
    #region Constructor

    public ArchivePatchCreatorViewModel()
    {
        ID = PatchFile.GenerateID();

        LoadOperation = new BindableOperation();

        BrowseThumbnailCommand = new AsyncRelayCommand(BrowseThumbnailAsync);
        RemoveThumbnailCommand = new RelayCommand(RemoveThumbnail);
        AddFileCommand = new RelayCommand(AddFile);
        AddFileFromFolderCommand = new AsyncRelayCommand(AddFileFromFolderAsync);
    }

    #endregion

    #region Private Fields

    private TempDirectory? _tempDir;

    #endregion

    #region Commands

    public ICommand BrowseThumbnailCommand { get; }
    public ICommand RemoveThumbnailCommand { get; }
    public ICommand AddFileCommand { get; }
    public ICommand AddFileFromFolderCommand { get; }

    #endregion

    #region Public Properties

    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    public int Revision { get; set; }
    public string ID { get; set; }
    public BitmapSource? Thumbnail { get; set; }

    public ObservableCollection<FileViewModel> Files { get; } = new();
    public FileViewModel? SelectedFile { get; set; }

    public bool IsImported { get; set; }

    public BindableOperation LoadOperation { get; }

    #endregion

    #region Public Methods

    public async Task<bool> ImportFromPatchAsync(FileSystemPath patchFilePath)
    {
        // TODO-UPDATE: Localize
        using (DisposableOperation operation = await LoadOperation.RunAsync("Importing from existing patch"))
        {
            // TODO-UPDATE: Try/catch

            using PatchFile patchFile = new(patchFilePath, true);

            PatchManifest manifest = patchFile.ReadManifest();

            if (manifest.PatchVersion > PatchFile.Version)
            {
                // TODO-UPDATE: Localize
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
                for (var i = 0; i < manifest.AddedFiles.Length; i++)
                {
                    operation.SetProgress(new Progress(i, manifest.AddedFiles.Length));

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
                }

                operation.SetProgress(new Progress(manifest.AddedFiles.Length, manifest.AddedFiles.Length));
            }

            IsImported = true;

            return true;
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
        using (await LoadOperation.RunAsync("Creating patch"))
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
        public string ArchiveFilePath { get; set; } = String.Empty;

        public string? Checksum { get; set; }

        public bool IsSelected { get; set; }

        public bool IsValid => !ArchiveFilePath.IsNullOrWhiteSpace();
        public bool IsFileAdded => SourceFilePath.FileExists;
        public bool IsImported => Checksum != null;
    }

    #endregion
}
﻿using System;
using System.Collections.ObjectModel;
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
    }

    public ICommand BrowseThumbnailCommand { get; }
    public ICommand RemoveThumbnailCommand { get; }
    public ICommand AddFileCommand { get; }

    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    public int Revision { get; set; }
    public BitmapImage? Thumbnail { get; set; }

    public ObservableCollection<FileViewModel> Files { get; } = new();
    public FileViewModel? SelectedFile { get; set; }

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

    public class FileViewModel : BaseViewModel
    {
        public FileSystemPath SourceFilePath { get; set; }
        public string ArchiveFilePath { get; set; } = String.Empty;

        public bool IsSelected { get; set; }

        public bool IsValid => !ArchiveFilePath.IsNullOrWhiteSpace();
    }
}
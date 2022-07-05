using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ByteSizeLib;

namespace RayCarrot.RCP.Metro.Archive;

public class PatchViewModel : BaseViewModel, IDisposable
{
    public PatchViewModel(PatchContainerViewModel containerViewModel, PatchManifest manifest, bool isEnabled, IPatchDataSource dataSource)
    {
        ContainerViewModel = containerViewModel;
        Manifest = manifest;
        _isEnabled = isEnabled;
        DataSource = dataSource;

        // TODO-UPDATE: Localize
        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("Author", manifest.Author),
            new("Size", ByteSize.FromBytes(manifest.TotalSize).ToString()),
            new("Date", manifest.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
            new("Revision", manifest.Revision.ToString()),
            new("ID", manifest.ID, UserLevel.Technical),
            new("Version", manifest.PatchVersion.ToString(), UserLevel.Debug),
            new("Added Files", (manifest.AddedFiles?.Length ?? 0).ToString()),
            new("Removed Files", (manifest.RemovedFiles?.Length ?? 0).ToString()),
        };

        ExtractContentsCommand = new AsyncRelayCommand(async () => await ContainerViewModel.ExtractPatchContentsAsync(this));
        ExportCommand = new AsyncRelayCommand(async () => await ContainerViewModel.ExportPatchAsync(this));
        UpdateCommand = new AsyncRelayCommand(async () => await ContainerViewModel.UpdatePatchAsync(this));
        RemoveCommand = new RelayCommand(() => ContainerViewModel.RemovePatch(this));
    }

    public ICommand ExtractContentsCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand RemoveCommand { get; }

    private bool _isEnabled;

    public PatchContainerViewModel ContainerViewModel { get; }
    public PatchManifest Manifest { get; }
    public IPatchDataSource DataSource { get; }
    public ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }
    public ImageSource? Thumbnail { get; private set; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            ContainerViewModel.RefreshPatchedFiles();
        }
    }

    // Currently unused, but can be used to allow patches to be downloaded
    public string? PatchURL { get; set; }
    [MemberNotNullWhen(true, nameof(PatchURL))]
    public bool IsDownloaded { get; set; }
    public bool IsDownloadable => PatchURL != null && !IsDownloaded;

    public void LoadThumbnail(PatchContainerFile? container)
    {
        // TODO-UPDATE: Log

        if (!Manifest.HasAsset(PatchAsset.Thumbnail))
        {
            Thumbnail = null;
            return;
        }

        using Stream thumbStream = DataSource.GetAsset(PatchAsset.Thumbnail);

        // This doesn't seem to work when reading from a zip archive as read-only due to the stream
        // not supporting seeking. Specifying the format directly using a PngBitmapDecoder still works.
        //Thumbnail = BitmapFrame.Create(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        Thumbnail = new PngBitmapDecoder(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.FirstOrDefault();

        if (Thumbnail?.CanFreeze == true)
            Thumbnail.Freeze();
    }

    public void Dispose()
    {
        DataSource.Dispose();
    }
}
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
    public PatchViewModel(PatchContainerViewModel containerViewModel, PatchManifest manifest, bool isEnabled, Patch? patch)
    {
        ContainerViewModel = containerViewModel;
        Manifest = manifest;
        _isEnabled = isEnabled;
        Patch = patch;

        // TODO-UPDATE: Localize
        // TODO-UPDATE: Show id for debug
        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("Author", manifest.Author),
            new("Flags", manifest.Flags.ToString()),
            new("Size", ByteSize.FromBytes(manifest.TotalSize).ToString()),
            new("Date", manifest.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
            new("Revision", manifest.Revision.ToString()),
            new("Added Files", (manifest.AddedFiles?.Length ?? 0).ToString()),
            new("Removed Files", (manifest.RemovedFiles?.Length ?? 0).ToString()),
        };

        RemoveCommand = new RelayCommand(() => ContainerViewModel.RemovePatch(this));
    }

    public ICommand RemoveCommand { get; }

    private bool _isEnabled;

    public PatchContainerViewModel ContainerViewModel { get; }
    public PatchManifest Manifest { get; }
    public Patch? Patch { get; }
    public ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }
    public ImageSource? Thumbnail { get; private set; }

    [MemberNotNullWhen(true, nameof(Patch))]
    public bool IsPendingImport => Patch != null;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            ContainerViewModel.RefreshPatchedFiles();
        }
    }

    public void LoadThumbnail(PatchContainer? container)
    {
        // TODO-UPDATE: Log

        if (!Manifest.HasAsset(PatchAsset.Thumbnail))
        {
            Thumbnail = null;
            return;
        }

        using Stream? thumbStream = container?.GetPatchAsset(Manifest.ID, PatchAsset.Thumbnail) ??
                                   Patch?.GetPatchAsset(PatchAsset.Thumbnail) ?? null;

        if (thumbStream == null)
        {
            Thumbnail = null;
            return;
        }

        // This doesn't seem to work when reading from a zip archive as read-only due to the stream
        // not supporting seeking. Specifying the format directly using a PngBitmapDecoder still works.
        //Thumbnail = BitmapFrame.Create(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        Thumbnail = new PngBitmapDecoder(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.FirstOrDefault();

        if (Thumbnail?.CanFreeze == true)
            Thumbnail.Freeze();
    }

    public void Dispose()
    {
        Patch?.Dispose();
    }
}
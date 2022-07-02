using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ByteSizeLib;

namespace RayCarrot.RCP.Metro.Archive;

public class PatchViewModel : BaseViewModel
{
    public PatchViewModel(PatchContainerViewModel containerViewModel, PatchManifest patch, bool isEnabled)
    {
        ContainerViewModel = containerViewModel;
        Patch = patch;
        _isEnabled = isEnabled;

        // TODO-UPDATE: Localize
        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("Author", patch.Author),
            new("Flags", patch.Flags.ToString()),
            new("Size", ByteSize.FromBytes(patch.TotalSize).ToString()),
            new("Date", patch.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
            new("Revision", patch.Revision.ToString()),
            new("Added Files", (patch.AddedFiles?.Length ?? 0).ToString()),
            new("Removed Files", (patch.RemovedFiles?.Length ?? 0).ToString()),
        };

        RemoveCommand = new RelayCommand(() => ContainerViewModel.RemovePatch(this));
    }

    public ICommand RemoveCommand { get; }

    private bool _isEnabled;

    public PatchContainerViewModel ContainerViewModel { get; }
    public PatchManifest Patch { get; }
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

    public void LoadThumbnail()
    {
        using Stream? thumbStream = ContainerViewModel.Container.GetPatchThumbnail(Patch.ID);
        
        if (thumbStream == null)
        {
            Thumbnail = null;
            return;
        }

        Thumbnail = BitmapFrame.Create(thumbStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        Thumbnail.Freeze();
    }
}
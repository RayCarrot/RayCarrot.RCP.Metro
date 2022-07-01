using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using ByteSizeLib;

namespace RayCarrot.RCP.Metro.Archive;

public class PatchViewModel : BaseViewModel
{
    public PatchViewModel(PatchContainerViewModel containerViewModel, PatchManifestItem patch)
    {
        ContainerViewModel = containerViewModel;
        Patch = patch;
        _isEnabled = patch.IsEnabled;

        // TODO-UPDATE: Localize
        // Dummy data for testing
        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new DuoGridItemViewModel("Author", patch.Author),
            new DuoGridItemViewModel("Flags", patch.Flags.ToString()),
            new DuoGridItemViewModel("Size", ByteSize.FromBytes(patch.TotalSize).ToString()),
            new DuoGridItemViewModel("Date", patch.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
            new DuoGridItemViewModel("Revision", patch.Revision.ToString()),
            new DuoGridItemViewModel("Added Files", (patch.AddedFiles?.Length ?? 0).ToString()),
            new DuoGridItemViewModel("Removed Files", (patch.RemovedFiles?.Length ?? 0).ToString()),
        };

        RemoveCommand = new RelayCommand(() => ContainerViewModel.RemovePatch(this));
    }

    public ICommand RemoveCommand { get; }

    private bool _isEnabled;

    public PatchContainerViewModel ContainerViewModel { get; }
    public PatchManifestItem Patch { get; }
    public ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            ContainerViewModel.RefreshPatchedFiles();
        }
    }
}
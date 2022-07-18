using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using ByteSizeLib;

namespace RayCarrot.RCP.Metro.Patcher;

public class ExternalPatchViewModel : PatchViewModel
{
    public ExternalPatchViewModel(PatcherViewModel patcherViewModel, ExternalPatchManifest externalManifest) : base(patcherViewModel)
    {
        ExternalManifest = externalManifest;
        
        PatchInfo = new ObservableCollection<DuoGridItemViewModel>()
        {
            new("Author", externalManifest.Author),
            new("Size", ByteSize.FromBytes(externalManifest.TotalSize).ToString()),
            new("Download Size", ByteSize.FromBytes(externalManifest.PatchSize).ToString()),
            new("Date", externalManifest.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
            new("Revision", externalManifest.Revision.ToString()),
            new("ID", externalManifest.ID, UserLevel.Debug),
            new("Added Files", (externalManifest.AddedFilesCount).ToString()),
            new("Removed Files", (externalManifest.RemovedFilesCount).ToString()),
        };

        DownloadCommand = new AsyncRelayCommand(() => PatcherViewModel.DownloadPatchAsync(ExternalManifest));
    }

    public ICommand DownloadCommand { get; }

    public override string ID => ExternalManifest.ID;
    public override string Name => ExternalManifest.Name ?? String.Empty;
    public override string Description => ExternalManifest.Description ?? String.Empty;
    public override ObservableCollection<DuoGridItemViewModel> PatchInfo { get; }

    public ExternalPatchManifest ExternalManifest { get; }
}
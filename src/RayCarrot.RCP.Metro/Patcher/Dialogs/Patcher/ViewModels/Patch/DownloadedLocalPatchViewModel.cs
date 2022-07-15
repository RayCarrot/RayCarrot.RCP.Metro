namespace RayCarrot.RCP.Metro.Patcher;

public class DownloadedLocalPatchViewModel : LocalPatchViewModel
{
    public DownloadedLocalPatchViewModel(
        PatcherViewModel patcherViewModel, 
        PatchManifest manifest, 
        bool isEnabled, 
        IPatchDataSource dataSource,
        TempDirectory tempDirectory) : base(patcherViewModel, manifest, isEnabled, dataSource)
    {
        TempDirectory = tempDirectory;
    }

    public TempDirectory TempDirectory { get; }

    public override void Dispose()
    {
        base.Dispose();
        TempDirectory.Dispose();
    }
}
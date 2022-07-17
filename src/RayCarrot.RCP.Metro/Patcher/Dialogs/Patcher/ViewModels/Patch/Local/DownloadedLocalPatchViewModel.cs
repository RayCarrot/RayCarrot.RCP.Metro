namespace RayCarrot.RCP.Metro.Patcher;

public class DownloadedLocalPatchViewModel : PendingImportedLocalPatchViewModel
{
    public DownloadedLocalPatchViewModel(
        PatcherViewModel patcherViewModel, 
        PatchManifest manifest, 
        bool isEnabled,
        FileSystemPath patchFilePath,
        TempDirectory tempDirectory) : base(patcherViewModel, manifest, isEnabled, patchFilePath)
    {
        TempDirectory = tempDirectory;
    }

    public TempDirectory TempDirectory { get; }
    public override bool MovePatch => true;

    public override void Dispose()
    {
        base.Dispose();
        TempDirectory.Dispose();
    }
}
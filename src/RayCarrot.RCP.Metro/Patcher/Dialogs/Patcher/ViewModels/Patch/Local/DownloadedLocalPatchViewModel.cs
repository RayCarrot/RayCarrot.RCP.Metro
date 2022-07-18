namespace RayCarrot.RCP.Metro.Patcher;

public class DownloadedLocalPatchViewModel : PendingImportedLocalPatchViewModel
{
    public DownloadedLocalPatchViewModel(
        PatcherViewModel patcherViewModel,
        PatchFile patchFile, 
        bool isEnabled,
        FileSystemPath patchFilePath,
        TempDirectory tempDirectory) : base(patcherViewModel, patchFile, isEnabled, patchFilePath)
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
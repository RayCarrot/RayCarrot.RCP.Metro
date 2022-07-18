namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A local patch which is pending to be imported
/// </summary>
public class PendingImportedLocalPatchViewModel : LocalPatchViewModel
{
    public PendingImportedLocalPatchViewModel(PatcherViewModel patcherViewModel, PatchFile patchFile, bool isEnabled, FileSystemPath patchFilePath) 
        : base(patcherViewModel, patchFile, patchFilePath, isEnabled) { }

    /// <summary>
    /// True if the patch should be moved when added to the library. False if it should be copied.
    /// </summary>
    public virtual bool MovePatch => false;
}
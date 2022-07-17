namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A local patch which is pending to be imported
/// </summary>
public class PendingImportedLocalPatchViewModel : LocalPatchViewModel
{
    public PendingImportedLocalPatchViewModel(PatcherViewModel patcherViewModel, PatchManifest manifest, bool isEnabled, FileSystemPath patchFilePath) 
        : base(patcherViewModel, manifest, isEnabled)
    {
        PatchFilePath = patchFilePath;
    }

    /// <summary>
    /// The path of the file to be imported
    /// </summary>
    public FileSystemPath PatchFilePath { get; }

    /// <summary>
    /// True if the patch should be moved when added to the library. False if it should be copied.
    /// </summary>
    public virtual bool MovePatch => false;

    public override PatchFile ReadPatchFile() => new(PatchFilePath, readOnly: true);
}
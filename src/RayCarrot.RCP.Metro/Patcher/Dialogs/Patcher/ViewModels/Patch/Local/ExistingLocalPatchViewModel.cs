namespace RayCarrot.RCP.Metro.Patcher;

public class ExistingLocalPatchViewModel : LocalPatchViewModel
{
    public ExistingLocalPatchViewModel(PatcherViewModel patcherViewModel, PatchManifest manifest, bool isEnabled, PatchLibrary library) 
        : base(patcherViewModel, manifest, isEnabled)
    {
        Library = library;
    }

    public PatchLibrary Library { get; }

    public override PatchFile ReadPatchFile() => Library.ReadPatchFile(Manifest.ID);
}
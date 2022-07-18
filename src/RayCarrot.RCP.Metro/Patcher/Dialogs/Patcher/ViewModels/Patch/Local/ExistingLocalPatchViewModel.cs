namespace RayCarrot.RCP.Metro.Patcher;

public class ExistingLocalPatchViewModel : LocalPatchViewModel
{
    public ExistingLocalPatchViewModel(PatcherViewModel patcherViewModel, PatchFile patchFile, bool isEnabled, PatchLibrary library) 
        : base(patcherViewModel, patchFile, library.GetPatchFilePath(patchFile.Metadata.ID), isEnabled)
    {
        Library = library;
    }

    public PatchLibrary Library { get; }
}
using System;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchedFileViewModel : BaseViewModel
{
    public PatchedFileViewModel(PatchFilePath filePath, PatchedFileModification modification, PatchManifest patch)
    {
        FilePath = filePath;
        Modification = modification;
        Patch = patch ?? throw new ArgumentNullException(nameof(patch));
        OverridenPatches = new ObservableCollection<PatchManifest>();
    }

    public PatchFilePath FilePath { get; }
    public PatchedFileModification Modification { get; }
    public PatchManifest Patch { get; }
    public ObservableCollection<PatchManifest> OverridenPatches { get; }

    public enum PatchedFileModification
    {
        Add,
        Remove,
    }
}
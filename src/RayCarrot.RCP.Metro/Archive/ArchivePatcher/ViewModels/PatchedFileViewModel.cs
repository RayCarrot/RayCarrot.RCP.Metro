using System;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro.Archive;

public class PatchedFileViewModel : BaseViewModel
{
    public PatchedFileViewModel(string filePath, PatchedFileModification modification, PatchManifestItem patch)
    {
        FilePath = filePath;
        Modification = modification;
        Patch = patch ?? throw new ArgumentNullException(nameof(patch));
        OverridenPatches = new ObservableCollection<PatchManifestItem>();
    }

    public string FilePath { get; }
    public PatchedFileModification Modification { get; }
    public PatchManifestItem Patch { get; }
    public ObservableCollection<PatchManifestItem> OverridenPatches { get; }

    public enum PatchedFileModification
    {
        Add,
        Remove,
    }
}
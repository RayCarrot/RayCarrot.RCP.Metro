using System;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro.Patcher;

// TODO: After loading patcher UI, enumerate async every modified file and verify checksum. Update UI with checkmark if ok or warning
//       symbol if not. Keep hash set of not okay files for when refreshing. Always ok for new modifications.
public class PatchedFileViewModel : BaseViewModel
{
    public PatchedFileViewModel(PatchFilePath filePath, PatchedFileModification modification, PatchMetadata patchMetadata)
    {
        FilePath = filePath;
        LocationDisplayName = filePath.Location == String.Empty ? "Game" : filePath.Location; // TODO-UPDATE: Localize
        Modification = modification;
        PatchMetadata = patchMetadata ?? throw new ArgumentNullException(nameof(patchMetadata));
        OverridenPatches = new ObservableCollection<PatchMetadata>();
    }

    public PatchFilePath FilePath { get; }
    public LocalizedString LocationDisplayName { get; }
    public PatchedFileModification Modification { get; }
    public PatchMetadata PatchMetadata { get; }
    public ObservableCollection<PatchMetadata> OverridenPatches { get; }

    public enum PatchedFileModification
    {
        Add,
        Remove,
    }
}
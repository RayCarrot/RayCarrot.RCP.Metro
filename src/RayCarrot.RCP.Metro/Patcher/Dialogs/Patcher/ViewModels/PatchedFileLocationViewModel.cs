using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchedFileLocationViewModel : BaseViewModel
{
    public PatchedFileLocationViewModel(string location, IEnumerable<PatchedFileViewModel> patchedFiles)
    {
        Location = location;
        DisplayName = location == String.Empty ? "Game" : location; // TODO-UPDATE: Localize
        PatchedFiles = new ObservableCollection<PatchedFileViewModel>(patchedFiles);
    }

    public string Location { get; }
    public LocalizedString DisplayName { get; }
    public ObservableCollection<PatchedFileViewModel> PatchedFiles { get; }
}
#nullable disable
using System;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro;

public class Mod_RRR_BFModToggleViewModel : BaseRCPViewModel, IDisposable
{
    public Mod_RRR_BFModToggleViewModel(LocalizedString header, Mod_RRR_BigFilePatch patch, bool isDefaultToggled, ObservableCollection<LocalizedString> selectionOptions = null) : this(header, new Mod_RRR_BigFilePatch[] { patch }, isDefaultToggled, selectionOptions)
    { }

    public Mod_RRR_BFModToggleViewModel(LocalizedString header, Mod_RRR_BigFilePatch[] patches, bool isDefaultToggled, ObservableCollection<LocalizedString> selectionOptions = null)
    {
        Header = header;
        Patches = patches;
        IsDefaultToggled = isDefaultToggled;
        SelectionOptions = selectionOptions;
        SelectedSelectionIndex = 0;
    }

    public LocalizedString Header { get; }
    public Mod_RRR_BigFilePatch[] Patches { get; }
    public bool IsDefaultToggled { get; }
    public bool IsToggled { get; set; }

    public ObservableCollection<LocalizedString> SelectionOptions { get; }
    public int SelectedSelectionIndex { get; set; }

    public int SelectedPatch
    {
        get
        {
            if (!IsToggled)
                return 0;

            if (SelectionOptions != null)
                return SelectedSelectionIndex + 1;
            else
                return 1;
        }
        set
        {
            if (value <= 0)
            {
                IsToggled = false;
                SelectedSelectionIndex = 0;
            }
            else
            {
                IsToggled = true;

                if (SelectionOptions != null)
                    SelectedSelectionIndex = value - 1;
            }
        }
    }

    public void Dispose()
    {
        Header?.Dispose();
    }
}
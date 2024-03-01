namespace RayCarrot.RCP.Metro.Games.Tools.PrototypeRestoration;

public class BFModToggleViewModel : BaseViewModel, IDisposable
{
    public BFModToggleViewModel(LocalizedString header, BigFilePatch patch, bool isDefaultToggled, ObservableCollection<LocalizedString>? selectionOptions = null) : this(header, new[] { patch }, isDefaultToggled, selectionOptions)
    { }

    public BFModToggleViewModel(LocalizedString header, BigFilePatch[] patches, bool isDefaultToggled, ObservableCollection<LocalizedString>? selectionOptions = null)
    {
        Header = header;
        Patches = patches;
        IsDefaultToggled = isDefaultToggled;
        SelectionOptions = selectionOptions;
        SelectedSelectionIndex = 0;
    }

    public LocalizedString Header { get; }
    public BigFilePatch[] Patches { get; }
    public bool IsDefaultToggled { get; }
    public bool IsToggled { get; set; }

    public ObservableCollection<LocalizedString>? SelectionOptions { get; }
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
        Header.Dispose();
    }
}
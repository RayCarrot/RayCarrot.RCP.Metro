namespace RayCarrot.RCP.Metro.Games.Tools.PrototypeRestoration;

public class MemoryModToggleViewModel : BaseViewModel, IDisposable
{
    public MemoryModToggleViewModel(
        GameInstallation gameInstallation,
        LocalizedString header, 
        LocalizedString description, 
        Action<bool> toggleAction, 
        bool isToggled, 
        ObservableCollection<LocalizedString>? selectionOptions = null, 
        Action<int>? selectionAction = null)
    {
        GameInstallation = gameInstallation;
        Header = header;
        Description = description;
        ToggleAction = toggleAction;
        DefaultIsToggled = isToggled;
        SelectionOptions = selectionOptions;
        SelectionAction = selectionAction;
    }

    private bool _isToggled;
    private int _selectedSelectionIndex;

    public string? ID { get; private set; }

    public GameInstallation GameInstallation { get; }
    public LocalizedString Header { get; }
    public LocalizedString Description { get; }
    public Action<bool> ToggleAction { get; }
    public bool IsToggled
    {
        get => _isToggled;
        set
        {
            _isToggled = value;
            SaveState();
            ToggleAction(value);
        }
    }
    public bool DefaultIsToggled { get; }
    public ObservableCollection<LocalizedString>? SelectionOptions { get; }
    public Action<int>? SelectionAction { get; }
    public int SelectedSelectionIndex
    {
        get => _selectedSelectionIndex;
        set
        {
            _selectedSelectionIndex = value;
            SaveState();
            SelectionAction?.Invoke(value);
        }
    }

    private void SaveState()
    {
        if (ID == null)
            return;

        GameInstallation.ModifyObject<PrototypeRestorationData>(GameDataKey.RRR_PrototypeRestorationData, x =>
            x.ToggleStates[ID] = new ToggleState(IsToggled, SelectedSelectionIndex));
    }

    public void Init(string id)
    {
        // Set the ID
        ID = id;

        PrototypeRestorationData? data = GameInstallation.GetObject<PrototypeRestorationData>(GameDataKey.RRR_PrototypeRestorationData);

        // Attempt to restore saved values
        if (data != null && data.ToggleStates.TryGetValue(id, out ToggleState? state))
        {
            _isToggled = state.IsToggled;
            _selectedSelectionIndex = state.SelectionIndex;
        }
        else
        {
            _isToggled = DefaultIsToggled;
            _selectedSelectionIndex = 0;
        }

        // Invoke actions
        ToggleAction(_isToggled);
        SelectionAction?.Invoke(_selectedSelectionIndex);
    }

    public void Dispose()
    {
        Header.Dispose();
        Description.Dispose();
    }
}
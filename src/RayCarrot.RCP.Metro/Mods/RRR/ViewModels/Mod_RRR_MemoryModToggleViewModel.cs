using System;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro;

public class Mod_RRR_MemoryModToggleViewModel : BaseRCPViewModel, IDisposable
{
    public Mod_RRR_MemoryModToggleViewModel(LocalizedString header, LocalizedString description, Action<bool> toggleAction, bool isToggled, ObservableCollection<LocalizedString>? selectionOptions = null, Action<int>? selectionAction = null)
    {
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

        Data.Mod_RRR_ToggleStates[ID] = new UserData_Mod_RRR_ToggleState(IsToggled, SelectedSelectionIndex);
    }

    public void Init(string id)
    {
        // Set the ID
        ID = id;

        // Attempt to restore saved values
        if (Data.Mod_RRR_ToggleStates.ContainsKey(id))
        {
            _isToggled = Data.Mod_RRR_ToggleStates[id].IsToggled;
            _selectedSelectionIndex = Data.Mod_RRR_ToggleStates[id].SelectionIndex;
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
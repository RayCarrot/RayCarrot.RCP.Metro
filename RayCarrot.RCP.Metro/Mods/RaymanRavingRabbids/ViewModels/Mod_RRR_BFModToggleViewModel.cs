using System;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro
{
    public class Mod_RRR_BFModToggleViewModel : BaseRCPViewModel, IDisposable
    {
        public Mod_RRR_BFModToggleViewModel(LocalizedString header, Action<bool> toggleAction, bool isToggled, ObservableCollection<LocalizedString> selectionOptions = null, Action<int> selectionAction = null)
        {
            Header = header;
            ToggleAction = toggleAction;
            IsToggled = isToggled;
            SelectionOptions = selectionOptions;
            SelectionAction = selectionAction;
            SelectedSelectionIndex = 0;
        }

        private bool _isToggled;
        private int _selectedSelectionIndex;

        public LocalizedString Header { get; }
        public Action<bool> ToggleAction { get; }
        public bool IsToggled
        {
            get => _isToggled;
            set
            {
                _isToggled = value;
                ToggleAction(value);
            }
        }
        public ObservableCollection<LocalizedString> SelectionOptions { get; }
        public Action<int> SelectionAction { get; }
        public int SelectedSelectionIndex
        {
            get => _selectedSelectionIndex;
            set
            {
                _selectedSelectionIndex = value;
                SelectionAction(value);
            }
        }

        public void Dispose()
        {
            Header?.Dispose();
        }
    }
}
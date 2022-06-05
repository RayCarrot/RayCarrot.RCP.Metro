using System;

namespace RayCarrot.RCP.Metro;

public class EditorBoolFieldViewModel : EditorFieldViewModel<bool>
{
    public EditorBoolFieldViewModel(
        LocalizedString header, LocalizedString? info, 
        Func<bool> getValueAction, Action<bool> setValueAction) : base(header, info, getValueAction, setValueAction) { }
}
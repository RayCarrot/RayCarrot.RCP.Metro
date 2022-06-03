using System;

namespace RayCarrot.RCP.Metro;

public class EditorStringFieldViewModel : EditorFieldViewModel<string>
{
    public EditorStringFieldViewModel(
        LocalizedString header, LocalizedString info, 
        Func<string?> getValueAction, Action<string?> setValueAction, 
        int maxLength = Int32.MaxValue) : base(header, info, getValueAction, setValueAction)
    {
        MaxLength = maxLength;
    }

    public int MaxLength { get; }
}
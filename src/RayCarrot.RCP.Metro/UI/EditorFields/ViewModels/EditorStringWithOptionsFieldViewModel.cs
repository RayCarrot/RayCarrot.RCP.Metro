namespace RayCarrot.RCP.Metro;

public class EditorStringWithOptionsFieldViewModel : EditorStringFieldViewModel
{
    public EditorStringWithOptionsFieldViewModel(
        LocalizedString header, LocalizedString? info,
        Func<string?> getValueAction, Action<string?> setValueAction,
        IEnumerable<string> options,
        int maxLength = Int32.MaxValue) : base(header, info, getValueAction, setValueAction, maxLength)
    {
        Options = new ObservableCollection<string>(options);
    }

    public ObservableCollection<string> Options { get; }
}
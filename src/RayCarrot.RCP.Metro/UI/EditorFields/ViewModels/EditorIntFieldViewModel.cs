namespace RayCarrot.RCP.Metro;

public class EditorIntFieldViewModel : EditorFieldViewModel<long>
{
    public EditorIntFieldViewModel(
        LocalizedString header, LocalizedString? info, 
        Func<long> getValueAction, Action<long> setValueAction,
        long min = 0, long max = Int32.MaxValue) : base(header, info, getValueAction, setValueAction)
    {
        Min = min;
        Max = max;

        // Set initial value to avoid unnecessary settings from UI when default value is not within min/max range
        _value = min;
    }

    public long Min { get; }
    public long Max { get; }
}
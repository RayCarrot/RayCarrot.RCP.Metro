namespace RayCarrot.RCP.Metro;

public class EditorIntFieldViewModel : EditorFieldViewModel<long>
{
    public EditorIntFieldViewModel(
        LocalizedString header, 
        LocalizedString? info, 
        Func<long> getValueAction, 
        Action<long> setValueAction,
        Func<long>? getMinAction = null, 
        Func<long>? getMaxAction = null) 
        : base(header, info, getValueAction, setValueAction)
    {
        GetMinAction = getMinAction;
        GetMaxAction = getMaxAction;
        Min = 0;
        Max = Int32.MaxValue;
    }

    protected Func<long>? GetMinAction { get; }
    protected Func<long>? GetMaxAction { get; }

    public long Min { get; set; }
    public long Max { get; set; }

    public override void Refresh()
    {
        base.Refresh();

        if (GetMinAction != null)
            Min = GetMinAction();
        if (GetMaxAction != null)
            Max = GetMaxAction();
    }
}
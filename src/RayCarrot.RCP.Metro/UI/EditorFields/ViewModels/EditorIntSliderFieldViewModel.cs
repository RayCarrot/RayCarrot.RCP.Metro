namespace RayCarrot.RCP.Metro;

public class EditorIntSliderFieldViewModel : EditorIntFieldViewModel
{
    public EditorIntSliderFieldViewModel(
        LocalizedString header, 
        LocalizedString? info, 
        Func<long> getValueAction, 
        Action<long> setValueAction,
        Func<long>? getMinAction = null,
        Func<long>? getMaxAction = null) 
        : base(header, info, getValueAction, setValueAction, getMinAction, getMaxAction)
    { }
}
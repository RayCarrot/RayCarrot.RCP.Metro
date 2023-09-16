namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public abstract class SettingsSectionViewModel : BaseViewModel
{
    protected SettingsSectionViewModel(AppUserData data)
    {
        Data = data;
    }

    public AppUserData Data { get; }

    [PropertyChanged.DoNotCheckEquality]
    public bool IsSelected { get; set; }

    public abstract LocalizedString Header { get; }
    public abstract GenericIconKind Icon { get; }
    public virtual UserLevel UserLevel => UserLevel.Normal;

    public virtual void Refresh() { }
}
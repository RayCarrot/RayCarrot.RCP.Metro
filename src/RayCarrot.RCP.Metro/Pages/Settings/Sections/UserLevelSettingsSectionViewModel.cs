namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class UserLevelSettingsSectionViewModel : SettingsSectionViewModel
{
    public UserLevelSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_UserLevel));
    public override GenericIconKind Icon => GenericIconKind.Settings_UserLevel;
}
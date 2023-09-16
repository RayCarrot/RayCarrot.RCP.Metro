namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class DebugSettingsSectionViewModel : SettingsSectionViewModel
{
    public DebugSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_DebugHeader));
    public override GenericIconKind Icon => GenericIconKind.Settings_Debug;
    public override UserLevel UserLevel => UserLevel.Debug;
}
namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class StartupSettingsSectionViewModel : SettingsSectionViewModel
{
    public StartupSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_StartupHeader));
    public override GenericIconKind Icon => GenericIconKind.Settings_Startup;
}
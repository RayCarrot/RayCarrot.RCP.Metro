namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class DesignSettingsSectionViewModel : SettingsSectionViewModel
{
    public DesignSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_DesignHeader));
    public override GenericIconKind Icon => GenericIconKind.Settings_Design;
}
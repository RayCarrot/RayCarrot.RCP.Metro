namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class GeneralSettingsSectionViewModel : SettingsSectionViewModel
{
    public GeneralSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_GeneralHeader));
    public override GenericIconKind Icon => GenericIconKind.Settings_General;
}
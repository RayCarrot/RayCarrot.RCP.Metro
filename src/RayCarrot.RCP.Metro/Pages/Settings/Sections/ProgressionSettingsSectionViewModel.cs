namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class ProgressionSettingsSectionViewModel : SettingsSectionViewModel
{
    public ProgressionSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Progression_Header));
    public override GenericIconKind Icon => GenericIconKind.Settings_Progression;
}
namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class ModLoaderSettingsSectionViewModel : SettingsSectionViewModel
{
    public ModLoaderSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Settings_ModLoader_Header));
    public override GenericIconKind Icon => GenericIconKind.Settings_ModLoader;
}
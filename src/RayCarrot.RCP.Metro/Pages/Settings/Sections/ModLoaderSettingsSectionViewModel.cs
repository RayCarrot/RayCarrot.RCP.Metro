namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class ModLoaderSettingsSectionViewModel : SettingsSectionViewModel
{
    public ModLoaderSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => "Mod Loader"; // TODO-LOC
    public override GenericIconKind Icon => GenericIconKind.Settings_ModLoader;
}
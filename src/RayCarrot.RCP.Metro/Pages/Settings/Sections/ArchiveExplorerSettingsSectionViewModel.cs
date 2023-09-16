namespace RayCarrot.RCP.Metro.Pages.Settings.Sections;

public class ArchiveExplorerSettingsSectionViewModel : SettingsSectionViewModel
{
    public ArchiveExplorerSettingsSectionViewModel(AppUserData data) : base(data) { }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Archive_Title));
    public override GenericIconKind Icon => GenericIconKind.Settings_ArchiveExplorer;
}
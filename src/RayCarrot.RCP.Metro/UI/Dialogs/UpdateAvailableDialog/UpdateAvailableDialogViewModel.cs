namespace RayCarrot.RCP.Metro;

public class UpdateAvailableDialogViewModel : BaseRCPViewModel
{
    public UpdateAvailableDialogViewModel(UpdaterCheckResult updaterCheckResult)
    {
        if (!updaterCheckResult.NewVersionAvailable)
            throw new Exception("No new version is available");

        // TODO-LOC: Rename UpdateAvailable_Info_NewVersionBeta to UpdateAvailable_Info_VersionBeta
        Changelog = updaterCheckResult.NewVersionChangelog;
        InfoItems = new ObservableCollection<DuoGridItemViewModel>()
        {
            new(new ResourceLocString(nameof(Resources.UpdateAvailable_Info_CurrentVersion)), Services.App.IsBeta
                ? new ResourceLocString(nameof(Resources.UpdateAvailable_Info_NewVersionBeta), AppViewModel.AppVersion)
                : AppViewModel.AppVersion.ToString()),
            new(new ResourceLocString(nameof(Resources.UpdateAvailable_Info_NewVersion)), updaterCheckResult.IsNewVersionBeta 
                ? new ResourceLocString(nameof(Resources.UpdateAvailable_Info_NewVersionBeta), updaterCheckResult.NewVersion) 
                : updaterCheckResult.NewVersion.ToString()),
            new(new ResourceLocString(nameof(Resources.UpdateAvailable_Info_ReleaseDate)), $"{updaterCheckResult.NewVersionDate:D}"),
            new(new ResourceLocString(nameof(Resources.UpdateAvailable_Info_Size)), BinaryHelpers.BytesToString(updaterCheckResult.NewVersionSize)),
            new(new ResourceLocString(nameof(Resources.UpdateAvailable_Info_Url)), updaterCheckResult.NewVersionUrl, UserLevel.Debug),
        };
    }

    public string Changelog { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }

    public bool RequestedInstallNewUpdate { get; set; }
}
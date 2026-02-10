namespace RayCarrot.RCP.Metro;

public class UpdateAvailableDialogViewModel : BaseRCPViewModel
{
    public UpdateAvailableDialogViewModel(UpdaterCheckResult updaterCheckResult)
    {
        if (!updaterCheckResult.NewVersionAvailable)
            throw new Exception("No new version is available");

        Changelog = updaterCheckResult.NewVersionChangelog;
        InfoItems = new ObservableCollection<DuoGridItemViewModel>()
        {
            // TODO-LOC
            new("Current Version", AppViewModel.AppVersion.ToString()),
            new("New Version", $"{updaterCheckResult.NewVersion}{(updaterCheckResult.IsNewVersionBeta ? " (BETA)" : "")}"),
            new("Release Date", $"{updaterCheckResult.NewVersionDate:D}"),
            new("Update Size", BinaryHelpers.BytesToString(updaterCheckResult.NewVersionSize)),
            new("Update Url", updaterCheckResult.NewVersionUrl, UserLevel.Debug),
        };
    }

    public string Changelog { get; }
    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }

    public bool RequestedInstallNewUpdate { get; set; }
}
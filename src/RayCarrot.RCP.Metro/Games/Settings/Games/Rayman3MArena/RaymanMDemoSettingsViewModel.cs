namespace RayCarrot.RCP.Metro.Games.Settings;

/// <summary>
/// View model for the Rayman M demo configuration
/// </summary>
public class RaymanMDemoSettingsViewModel : RaymanMSettingsViewModel
{
    public RaymanMDemoSettingsViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        AppDataManager.AppData.IsDemo = true;
    }

    protected override FilePatcher_Patch[]? RemoveDiscCheckPatches => null;
}
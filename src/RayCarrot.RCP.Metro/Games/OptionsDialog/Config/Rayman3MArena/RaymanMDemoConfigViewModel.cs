namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman M demo configuration
/// </summary>
public class RaymanMDemoConfigViewModel : RaymanMConfigViewModel
{
    public RaymanMDemoConfigViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        AppDataManager.AppData.IsDemo = true;
    }

    protected override FilePatcher_Patch[]? RemoveDiscCheckPatches => null;
}
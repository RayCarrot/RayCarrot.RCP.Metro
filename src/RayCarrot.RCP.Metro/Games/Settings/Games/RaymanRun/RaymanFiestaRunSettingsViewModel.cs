namespace RayCarrot.RCP.Metro.Games.Settings;

/// <summary>
/// View model for the Rayman Fiesta Run settings
/// </summary>
public class RaymanFiestaRunSettingsViewModel : BaseUbiArtRunSettingsViewModel
{
    public RaymanFiestaRunSettingsViewModel(GameInstallation gameInstallation, FileSystemPath saveDir, bool isUpc) 
        : base(gameInstallation, saveDir, isUpc) { }
}
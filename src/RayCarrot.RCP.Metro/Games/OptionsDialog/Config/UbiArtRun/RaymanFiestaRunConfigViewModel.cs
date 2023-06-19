namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman Fiesta Run configuration
/// </summary>
public class RaymanFiestaRunConfigViewModel : UbiArtRunBaseConfigViewModel
{
    public RaymanFiestaRunConfigViewModel(GameDescriptor gameDescriptor,
        GameInstallation gameInstallation,
        FileSystemPath saveDir,
        bool isUpc) 
        : base(gameDescriptor, gameInstallation, saveDir, isUpc) { }
}
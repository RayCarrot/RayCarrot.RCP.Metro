namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman Fiesta Run configuration
/// </summary>
public class RaymanFiestaRunConfigViewModel : UbiArtRunBaseConfigViewModel
{
    public RaymanFiestaRunConfigViewModel(WindowsPackageGameDescriptor gameDescriptor) : base(gameDescriptor) 
    { }

    protected override Task OnGameInfoModifiedAsync() => LoadPageAsync();
}
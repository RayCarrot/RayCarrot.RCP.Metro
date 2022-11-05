using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Fiesta Run configuration
/// </summary>
public class Config_RaymanFiestaRun_ViewModel : Config_UbiArtRun_BaseViewModel
{
    public Config_RaymanFiestaRun_ViewModel(WindowsPackageGameDescriptor gameDescriptor) : base(gameDescriptor) 
    { }

    protected override Task OnGameInfoModified() => LoadPageAsync();

    protected override object GetPageUI() => new Config_RaymanFiestaRun_Control()
    {
        DataContext = this
    };
}
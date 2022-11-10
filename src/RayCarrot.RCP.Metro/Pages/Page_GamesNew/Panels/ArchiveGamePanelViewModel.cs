using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class ArchiveGamePanelViewModel : GamePanelViewModel
{
    public override GenericIconKind Icon => GenericIconKind.GameDisplay_Archive;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Utilities_ArchiveExplorer_Header));
    
    protected override Task LoadAsyncImpl(GameInstallation gameInstallation)
    {

        return Task.CompletedTask;
    }
}
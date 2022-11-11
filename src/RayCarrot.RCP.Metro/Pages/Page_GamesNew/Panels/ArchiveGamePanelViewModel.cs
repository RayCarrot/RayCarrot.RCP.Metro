using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class ArchiveGamePanelViewModel : GamePanelViewModel
{
    public ArchiveGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    public override GenericIconKind Icon => GenericIconKind.GameDisplay_Archive;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Utilities_ArchiveExplorer_Header));
    
    protected override Task LoadAsyncImpl()
    {

        return Task.CompletedTask;
    }
}
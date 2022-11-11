using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class LinksGamePanelViewModel : GamePanelViewModel
{
    public LinksGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    public override GenericIconKind Icon => GenericIconKind.GamePanel_Links;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.LinksPageHeader));
    
    protected override Task LoadAsyncImpl()
    {

        return Task.CompletedTask;
    }
}
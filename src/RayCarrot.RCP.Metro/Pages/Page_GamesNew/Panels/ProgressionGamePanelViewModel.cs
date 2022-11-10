using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class ProgressionGamePanelViewModel : GamePanelViewModel
{
    public override GenericIconKind Icon => GenericIconKind.GamePanel_Progression;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Progression_Header));
    
    protected override Task LoadAsyncImpl(GameInstallation gameInstallation)
    {

        return Task.CompletedTask;
    }
}
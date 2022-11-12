using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class ProgressionGamePanelViewModel : GamePanelViewModel
{
    public ProgressionGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation) { }

    public override GenericIconKind Icon => GenericIconKind.GamePanel_Progression;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Progression_Header));
    public override bool CanRefresh => true;
    
    protected override Task LoadAsyncImpl()
    {

        return Task.CompletedTask;
    }
}
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class ProgressionGamePanelViewModel : GamePanelViewModel
{
    public ProgressionGamePanelViewModel(GameInstallation gameInstallation, GameProgressionManager progressionManager) 
        : base(gameInstallation)
    {
        ProgressionManager = progressionManager;
    }

    public override GenericIconKind Icon => GenericIconKind.GamePanel_Progression;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Progression_Header));
    public override bool CanRefresh => true;

    public GameProgressionManager ProgressionManager { get; }
    
    protected override async Task LoadAsyncImpl()
    {
        var dataSource = Services.Data.Backup_GameDataSources.TryGetValue(GameDescriptor.BackupName, ProgramDataSource.Auto);
        var fileSystem = new GameProgressionManager.PhysicalFileSystemWrapper(dataSource);

        await foreach (GameProgressionSlot slot in ProgressionManager.LoadSlotsAsync(fileSystem))
        {
            // TODO-UPDATE: Process slots
        }
    }
}
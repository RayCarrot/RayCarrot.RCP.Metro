using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Panels;

public class ProgressionGamePanelViewModel : GamePanelViewModel
{
    public ProgressionGamePanelViewModel(GameInstallation gameInstallation, GameProgressionManager progressionManager) 
        : base(gameInstallation)
    {
        ProgressionManager = progressionManager;
    }

    public override GenericIconKind Icon => GenericIconKind.GamePanel_Progression;
    public override LocalizedString Header => ProgressionManager.Name == null 
        ? new ResourceLocString(nameof(Resources.Progression_Header))
        : new GeneratedLocString(() => String.Format(Resources.Progression_HeaderWithSecondary, Resources.Progression_Header, ProgressionManager.Name));
    public override bool CanRefresh => true;

    public GameProgressionManager ProgressionManager { get; }
    public GameProgressionSlotViewModel? PrimarySlot { get; set; }

    protected override async Task LoadAsyncImpl()
    {
        ProgressionDataSources? dataSources = GameInstallation.GetObject<ProgressionDataSources>(GameDataKey.Progression_DataSources);
        var dataSource = dataSources?.DataSources.TryGetValue(ProgressionManager.ProgressionId, out ProgramDataSource src) == true
            ? src
            : ProgramDataSource.Auto;
        var fileSystem = new GameProgressionManager.PhysicalFileSystemWrapper(dataSource);

        GameProgressionSlot[] slots = await ProgressionManager.LoadSlotsAsync(fileSystem).ToArrayAsync();
        GameProgressionSlot? primarySlot = GameProgressionManager.CreatePrimarySlot(slots);

        PrimarySlot = primarySlot == null ? null : new GameProgressionSlotViewModel(primarySlot);

        if (PrimarySlot == null)
            IsEmpty = true;
    }

    public class GameProgressionSlotViewModel : BaseViewModel
    {
        public GameProgressionSlotViewModel(GameProgressionSlot slot)
        {
            Slot = slot;
            PrimaryDataItems = new ObservableCollection<GameProgressionDataItem>(slot.DataItems.Where(x => x.IsPrimaryItem));
        }

        public GameProgressionSlot Slot { get; }
        public double Percentage => Slot.Percentage;
        public GameProgressionSlot.ProgressionState State => Slot.State;
        public ObservableCollection<GameProgressionDataItem> PrimaryDataItems { get; }
    }
}
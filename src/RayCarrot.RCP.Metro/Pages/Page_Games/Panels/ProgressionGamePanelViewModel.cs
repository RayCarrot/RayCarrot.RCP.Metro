using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

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
        // TODO-UPDATE: Localize
        : new GeneratedLocString(() => $"{Resources.Progression_Header} ({ProgressionManager.Name})");
    public override bool CanRefresh => true;

    public GameProgressionManager ProgressionManager { get; }
    public GameProgressionSlotViewModel? PrimarySlot { get; set; }

    protected override async Task LoadAsyncImpl()
    {
        var dataSource = Services.Data.Backup_GameDataSources.TryGetValue(ProgressionManager.BackupName, ProgramDataSource.Auto);
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
        public Brush ProgressBrush
        {
            get
            {
                // TODO-UPDATE: Move to resource dictionary and do in xaml instead (same for progression page)
                if (Slot.Is100Percent)
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80));
                else if (Percentage >= 50)
                    return new SolidColorBrush(Color.FromRgb(255, 238, 88));
                else
                    return new SolidColorBrush(Color.FromRgb(239, 83, 80));
            }
        }
        public ObservableCollection<GameProgressionDataItem> PrimaryDataItems { get; }
    }
}
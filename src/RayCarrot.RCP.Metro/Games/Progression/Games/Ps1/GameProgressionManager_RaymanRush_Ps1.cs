using BinarySerializer.PS1.MemoryCard;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanRush_Ps1 : EmulatedGameProgressionManager
{
    public GameProgressionManager_RaymanRush_Ps1(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        DataBlock<RaymanRushPs1SaveData> saveBlock = await emulatedSave.ReadAsync<DataBlock<RaymanRushPs1SaveData>>();

        int levelsCount = saveBlock.SaveData.SaveBlock1.Levels.Length;

        // TODO: Maybe add lap/race times as well?
        List<GameProgressionDataItem> dataItems = new()
        {
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.RR_Championship,
                header: new ResourceLocString(nameof(Resources.Progression_RRushChampionship)),
                value: saveBlock.SaveData.SaveBlock1.Levels.Sum(x => x.CompletedChampionship != 0 ? 1 : 0),
                max: levelsCount),
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.RR_TimeAttack,
                header: new ResourceLocString(nameof(Resources.Progression_RRushTimeAttack)),
                value: saveBlock.SaveData.SaveBlock1.Levels.Sum(x => x.CompletedModes >= 2 ? 1 : 0),
                max: levelsCount),
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.RR_Lums,
                header: new ResourceLocString(nameof(Resources.Progression_RRushLums)),
                value: saveBlock.SaveData.SaveBlock1.Levels.Sum(x => x.CompletedModes >= 3 ? 1 : 0),
                max: levelsCount),
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.RR_Target,
                header: new ResourceLocString(nameof(Resources.Progression_RRushTarget)),
                value: saveBlock.SaveData.SaveBlock1.Levels.Sum(x => x.CompletedModes >= 4 ? 1 : 0),
                max: levelsCount),
        };

        yield return new SerializabeEmulatedGameProgressionSlot<DataBlock<RaymanRushPs1SaveData>>(
            name: saveBlock.SaveData.SaveBlock1.Name,
            index: -1,
            percentage: saveBlock.SaveData.SaveBlock1.Percentage / 10f,
            emulatedSave: emulatedSave,
            dataItems: dataItems,
            serializable: saveBlock)
        {
            GetExportObject = x => x.SaveData,
            SetImportObject = (x, o) => x.SaveData = (RaymanRushPs1SaveData)o,
            ExportedType = typeof(RaymanRushPs1SaveData)
        };

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
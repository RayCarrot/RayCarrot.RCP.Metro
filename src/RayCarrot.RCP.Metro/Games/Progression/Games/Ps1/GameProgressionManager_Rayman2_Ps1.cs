using BinarySerializer.PS1.MemoryCard;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman2_Ps1 : EmulatedGameProgressionManager
{
    public GameProgressionManager_Rayman2_Ps1(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        DataBlock<Rayman2Ps1SaveData> saveBlock = await emulatedSave.ReadAsync<DataBlock<Rayman2Ps1SaveData>>();

        int lums = saveBlock.SaveData.SaveBlock1.LumCounts.Sum(x => x);
        int cages = saveBlock.SaveData.SaveBlock1.CageCounts.Sum(x => x);

        List<GameProgressionDataItem> progressItems = new()
        {
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.R2_Lum,
                header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                value: lums,
                max: 800),
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.R2_Cage,
                header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                value: cages,
                max: 60),
        };

        if (saveBlock.SaveData.SaveBlock1.RaceTime1 != 0xFFFFFFFF)
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Clock,
                header: new ResourceLocString(nameof(Resources.R2_BonusLevelName_1)),
                text: $"{TimeSpan.FromSeconds(saveBlock.SaveData.SaveBlock1.RaceTime1 / 30d):mm\\:ss\\.ff}"));

        if (saveBlock.SaveData.SaveBlock1.RaceTime2 != 0xFFFFFFFF)
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Clock,
                header: new ResourceLocString(nameof(Resources.R2_BonusLevelName_2)),
                text: $"{TimeSpan.FromSeconds(saveBlock.SaveData.SaveBlock1.RaceTime2 / 30d):mm\\:ss\\.ff}"));

        yield return new SerializabeEmulatedGameProgressionSlot<DataBlock<Rayman2Ps1SaveData>>(
            name: null,
            index: -1,
            collectiblesCount: lums + cages,
            totalCollectiblesCount: 800 + 60,
            emulatedSave: emulatedSave,
            dataItems: progressItems,
            serializable: saveBlock)
        {
            GetExportObject = x => x.SaveData,
            SetImportObject = (x, o) => x.SaveData = (Rayman2Ps1SaveData)o,
            ExportedType = typeof(Rayman2Ps1SaveData)
        };

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
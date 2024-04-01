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

        yield return new SerializabeEmulatedGameProgressionSlot<DataBlock<RaymanRushPs1SaveData>>(
            name: saveBlock.SaveData.SaveBlock1.Name,
            index: 0,
            percentage: saveBlock.SaveData.SaveBlock1.Percentage / 10f,
            emulatedSave: emulatedSave,
            dataItems: Array.Empty<GameProgressionDataItem>(), // TODO-UPDATE: Add items
            serializable: saveBlock)
        {
            GetExportObject = x => x.SaveData,
            SetImportObject = (x, o) => x.SaveData = (RaymanRushPs1SaveData)o,
            ExportedType = typeof(RaymanRushPs1SaveData)
        };

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
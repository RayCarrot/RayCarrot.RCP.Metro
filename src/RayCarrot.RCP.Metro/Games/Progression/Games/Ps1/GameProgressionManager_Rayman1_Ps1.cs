using BinarySerializer.PlayStation.PS1.MemoryCard;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman1_Ps1 : EmulatedGameProgressionManager
{
    public GameProgressionManager_Rayman1_Ps1(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        DataBlock<SaveSlot> saveBlock = await emulatedSave.ReadAsync<DataBlock<SaveSlot>>();

        IReadOnlyList<GameProgressionDataItem> dataItems = Rayman1Progression.CreateProgressionItems(
            saveBlock.SaveData, out int collectiblesCount, out int maxCollectiblesCount);

        yield return new SerializabeEmulatedGameProgressionSlot<DataBlock<SaveSlot>>(
            name: (emulatedSave as EmulatedPs1Save)?.Name.Substring(0, 3).ToUpper(),
            index: -1,
            collectiblesCount: collectiblesCount,
            totalCollectiblesCount: maxCollectiblesCount,
            emulatedSave: emulatedSave,
            dataItems: dataItems,
            serializable: saveBlock)
        {
            GetExportObject = x => x.SaveData,
            SetImportObject = (x, o) => x.SaveData = (SaveSlot)o,
            ExportedType = typeof(SaveSlot)
        };

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
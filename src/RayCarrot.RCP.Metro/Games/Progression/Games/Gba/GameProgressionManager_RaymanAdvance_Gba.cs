using BinarySerializer.Ray1;
using BinarySerializer.Ray1.GBA;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanAdvance_Gba : EmulatedGameProgressionManager
{
    public GameProgressionManager_RaymanAdvance_Gba(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        SaveData saveData = await emulatedSave.ReadAsync<SaveData>();

        for (int saveIndex = 0; saveIndex < saveData.SaveSlots.Length; saveIndex++)
        {
            SaveSlot saveSlot = saveData.SaveSlots[saveIndex];

            if (!saveSlot.GBA_IsValid)
                continue;

            IReadOnlyList<GameProgressionDataItem> dataItems = Rayman1Progression.CreateProgressionItems(
                saveSlot, out int collectiblesCount, out int maxCollectiblesCount);

            int slotIndex = saveIndex;

            yield return new SerializabeEmulatedGameProgressionSlot<SaveData>(
                name: saveSlot.SaveName.ToUpper().Replace('~', '△'),
                index: saveIndex,
                collectiblesCount: collectiblesCount,
                totalCollectiblesCount: maxCollectiblesCount,
                emulatedSave: emulatedSave,
                dataItems: dataItems,
                serializable: saveData)
            {
                GetExportObject = x => x.SaveSlots[slotIndex],
                SetImportObject = (x, o) => x.SaveSlots[slotIndex] = (SaveSlot)o,
                ExportedType = typeof(SaveSlot)
            };
        }

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
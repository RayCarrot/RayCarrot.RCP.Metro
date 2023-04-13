using BinarySerializer.Ray1;
using BinarySerializer.Ray1.GBA;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanAdvance_Gba : EmulatedGameProgressionManager
{
    public GameProgressionManager_RaymanAdvance_Gba(GameInstallation gameInstallation, string backupId) 
        : base(gameInstallation, backupId) 
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

            // Get total amount of cages
            int cages = saveSlot.WorldInfoSaveZone.Sum(x => x.Cages);

            GameProgressionDataItem[] dataItems =
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R1_Cage,
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: cages,
                    max: 102),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.R1_Continue,
                    header: new ResourceLocString(nameof(Resources.Progression_Continues)),
                    value: saveSlot.ContinuesCount),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.R1_Life,
                    header: new ResourceLocString(nameof(Resources.Progression_Lives)),
                    value: saveSlot.StatusBar.LivesCount),
            };

            yield return new EmulatedGameProgressionSlot(
                name: saveSlot.SaveName.ToUpper(),
                index: saveIndex,
                collectiblesCount: cages,
                totalCollectiblesCount: 102,
                emulatedSave: emulatedSave,
                dataItems: dataItems);
        }

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman3_Gba : EmulatedGameProgressionManager
{
    public GameProgressionManager_Rayman3_Gba(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        R3GBA_SaveData saveData = await emulatedSave.ReadAsync<R3GBA_SaveData>();

        bool isPrototype = emulatedSave.Context.GetRequiredSettings<R3GBA_Settings>().IsPrototype;

        for (int saveIndex = 0; saveIndex < saveData.Slots.Length; saveIndex++)
        {
            R3GBA_SaveSlot saveSlot = saveData.Slots[saveIndex];

            if (saveSlot.Lives == 0)
                continue;

            // Get total amount of lums
            const int totalLums = 1000;
            int lums = 0;
            for (int i = 0; i < totalLums; i++)
            {
                if ((saveSlot.Lums[i >> 3] & (1 << (i & 7))) == 0)
                    lums++;
            }

            // Get total amount of cages
            const int totalCages = 50;
            int cages = 0;
            for (int i = 0; i < totalCages; i++)
            {
                if ((saveSlot.Cages[i >> 3] & (1 << (i & 7))) == 0)
                    cages++;
            }

            // Get total amount of completedGCN bonus levels
            int totalGCNBonusLevels = isPrototype ? 0 : 10;
            int completedGCNBonusLevels = isPrototype ? 0 : saveSlot.LastCompletedGCNBonus;

            List<GameProgressionDataItem> dataItems = new()
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R3_GBA_Lum,
                    header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                    value: lums,
                    max: totalLums),
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R3_GBA_Cage,
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: cages,
                    max: totalCages),
            };

            if (!isPrototype)
            {
                dataItems.Add(new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R3_GBA_GcnCheck,
                    header: new ResourceLocString(nameof(Resources.Progression_R3_GBA_CompletedGCNBonus)),
                    value: completedGCNBonusLevels,
                    max: totalGCNBonusLevels));
            }

            dataItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R3_GBA_Life,
                header: new ResourceLocString(nameof(Resources.Progression_Lives)),
                value: saveSlot.Lives));


            int slotIndex = saveIndex;

            yield return new SerializabeEmulatedGameProgressionSlot<R3GBA_SaveData>(
                name: null,
                index: saveIndex,
                collectiblesCount: lums + cages + completedGCNBonusLevels,
                totalCollectiblesCount: totalLums + totalCages + totalGCNBonusLevels,
                emulatedSave: emulatedSave,
                dataItems: dataItems,
                serializable: saveData)
            {
                GetExportObject = x => x.Slots[slotIndex],
                SetImportObject = (x, o) => x.Slots[slotIndex] = (R3GBA_SaveSlot)o,
                ExportedType = typeof(R3GBA_SaveSlot)
            };
        }

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
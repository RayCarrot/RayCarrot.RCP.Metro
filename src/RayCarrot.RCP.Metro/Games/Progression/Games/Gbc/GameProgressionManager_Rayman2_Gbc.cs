namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman2_Gbc : EmulatedGameProgressionManager
{
    public GameProgressionManager_Rayman2_Gbc(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        R2GBC_SaveData saveData = await emulatedSave.ReadAsync<R2GBC_SaveData>();

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            if (!saveData.ValidSlots[saveIndex])
                continue;

            R2GBC_SaveSlot saveSlot = saveData.Slots[saveIndex];

            // Get total amount of lums
            const int totalLums = 800;
            int lums = saveSlot.GetCollectedLumsCount();

            // Get total amount of cages
            const int totalCages = 30;
            int cages = saveSlot.GetTotalCollectedCages();

            // Get completed levels
            const int totalLevels = 32;
            int levels = saveSlot.GetLevel();

            List<GameProgressionDataItem> dataItems = new()
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R2_GBC_Level,
                    header: new ResourceLocString(nameof(Resources.Progression_LevelsCompleted)),
                    value: levels,
                    max: totalLevels),
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R2_GBC_Lum,
                    header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                    value: lums,
                    max: totalLums),
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R2_GBC_Cage,
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: cages,
                    max: totalCages),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.R2_GBC_Life,
                    header: new ResourceLocString(nameof(Resources.Progression_Lives)),
                    value: saveSlot.GetLivesCount()),
            };

            double percentage = 0;
            percentage += ((double)lums / totalLums) * 40;
            percentage += ((double)cages / totalCages) * 30;
            percentage += ((double)levels / totalLevels) * 30;

            if (percentage > 100)
                percentage = 100;

            int slotIndex = saveIndex;

            yield return new SerializabeEmulatedGameProgressionSlot<R2GBC_SaveData>(
                name: null,
                index: saveIndex,
                percentage: percentage,
                emulatedSave: emulatedSave,
                dataItems: dataItems,
                serializable: saveData)
            {
                GetExportObject = x => x.Slots[slotIndex],
                SetImportObject = (x, o) => x.Slots[slotIndex] = (R2GBC_SaveSlot)o,
                ExportedType = typeof(R2GBC_SaveSlot)
            };
        }

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
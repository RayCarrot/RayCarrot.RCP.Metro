namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanRavingRabbids_Gba : EmulatedGameProgressionManager
{
    public GameProgressionManager_RaymanRavingRabbids_Gba(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        RRRGBA_SaveData saveData = await emulatedSave.ReadAsync<RRRGBA_SaveData>();

        for (int saveIndex = 0; saveIndex < saveData.Slots.Length; saveIndex++)
        {
            RRRGBA_SaveSlot saveSlot = saveData.Slots[saveIndex];

            if (saveSlot.Name.IsNullOrEmpty())
                continue;

            // Get total amount of lums
            const int totalLums = 1550;
            int lums = 0;
            for (int i = 0; i < totalLums; i++)
            {
                if ((saveSlot.Lums[i >> 3] & (1 << (i & 7))) != 0)
                    lums++;
            }

            // Get total amount of cages
            const int totalCages = 60;
            int cages = 0;
            for (int i = 0; i < totalCages; i++)
            {
                if ((saveSlot.Cages[i >> 3] & (1 << (i & 7))) != 0)
                    cages++;
            }

            GameProgressionDataItem[] dataItems =
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.RRR_GBA_Lum,
                    header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                    value: lums,
                    max: totalLums),
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.RRR_GBA_Cage,
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: cages,
                    max: totalCages),
            };

            int slotIndex = saveIndex;

            yield return new SerializabeEmulatedGameProgressionSlot<RRRGBA_SaveData>(
                name: saveSlot.Name,
                index: saveIndex,
                collectiblesCount: lums + cages,
                totalCollectiblesCount: totalLums + totalCages,
                emulatedSave: emulatedSave,
                dataItems: dataItems,
                serializable: saveData)
            {
                GetExportObject = x => x.Slots[slotIndex],
                SetImportObject = (x, o) => x.Slots[slotIndex] = (RRRGBA_SaveSlot)o,
                ExportedType = typeof(RRRGBA_SaveSlot)
            };

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }

        List<GameProgressionDataItem> items = new();

        // Don't count first 2 levels since those are the intro levels and not available in time attack
        int completedTimeAttackLevels = 0;
        int maxTimeAttackLevels = saveData.LevelTimes.Length - 2;
        for (int i = 2; i < saveData.LevelTimes.Length; i++)
        {
            ushort seconds = saveData.LevelTimes[i];

            if (seconds == 0)
                continue;

            items.Add(new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.RRR_GBA_Time,
                header: $"Level {i}", // TODO-UPDATE: Localize with level names
                text: $"{new TimeSpan(0, 0, 0, seconds):mm\\:ss}"));
            completedTimeAttackLevels++;
        }

        items.Insert(0, new GameProgressionDataItem(
            isPrimaryItem: true,
            icon: ProgressionIconAsset.RRR_GBA_Time,
            header: Resources.Progression_LevelsCompleted,
            value: completedTimeAttackLevels,
            max: maxTimeAttackLevels));

        yield return new SerializabeEmulatedGameProgressionSlot<RRRGBA_SaveData>(
            name: "Time attack", // TODO-UPDATE: Localize
            index: 3,
            collectiblesCount: completedTimeAttackLevels,
            totalCollectiblesCount: maxTimeAttackLevels,
            emulatedSave: emulatedSave,
            dataItems: items,
            serializable: saveData)
        {
            SlotGroup = 1,
            GetExportObject = x => x.LevelTimes,
            SetImportObject = (x, o) => x.LevelTimes = (ushort[])o,
            ExportedType = typeof(ushort[])
        };

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
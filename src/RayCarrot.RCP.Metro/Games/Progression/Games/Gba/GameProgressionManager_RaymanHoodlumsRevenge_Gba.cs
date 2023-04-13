namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanHoodlumsRevenge_Gba : EmulatedGameProgressionManager
{
    public GameProgressionManager_RaymanHoodlumsRevenge_Gba(GameInstallation gameInstallation, string backupId) 
        : base(gameInstallation, backupId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        RHR_SaveData saveData = await emulatedSave.ReadAsync<RHR_SaveData>();

        for (int saveIndex = 0; saveIndex < saveData.Slots.Length; saveIndex++)
        {
            RHR_SaveSlot saveSlot = saveData.Slots[saveIndex];

            if ((saveSlot.Flags & RHR_SaveSlotFlags.InUse) == 0)
                continue;

            int totalLums = LevelInfos.Sum(x => x.Lums);
            int totalTeensies = LevelInfos.Sum(x => x.Teensies);
            int totalStamps = LevelInfos.Count(x => x.HasScore) * 3;
            int totalLevels = LevelInfos.Length;

            int lums = saveSlot.Lums;
            int teensies = saveSlot.Teensies;
            int stamps = 0;
            int completedLevels = saveSlot.Levels.Count(x => (x.Flags & RHR_LevelSaveFlags.Completed) != 0);

            // Calculate stamps since the game doesn't store these in the save
            for (int i = 0; i < saveSlot.Levels.Length; i++)
            {
                RHR_LevelSave levelSave = saveSlot.Levels[i];
                LevelInfo levelInfo = LevelInfos[i];

                if (!levelInfo.HasScore)
                    continue;

                if (levelSave.Score >= levelInfo.StampScore3)
                    stamps += 3;
                else if (levelSave.Score >= levelInfo.StampScore2)
                    stamps += 2;
                else if (levelSave.Score >= levelInfo.StampScore1)
                    stamps += 1;
            }

            GameProgressionDataItem[] dataItems =
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.RHR_MapComplete,
                    header: new ResourceLocString(nameof(Resources.Progression_LevelsCompleted)),
                    value: completedLevels,
                    max: totalLevels),
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.RHR_Lum,
                    header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                    value: lums,
                    max: totalLums),
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.RHR_Teensy,
                    header: new ResourceLocString(nameof(Resources.Progression_Teensies)),
                    value: teensies,
                    max: totalTeensies),
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.RHR_Stamp,
                    header: new ResourceLocString(nameof(Resources.Progression_R3Stamps)),
                    value: stamps,
                    max: totalStamps),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: new ResourceLocString(nameof(Resources.Progression_TotalScore)),
                    value: saveSlot.Score),

                // TODO-UPDATE: Localize
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Fairy Council",
                    value: saveSlot.Levels[0].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Clearleaf Forest",
                    value: saveSlot.Levels[1].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Clearleaf Falls",
                    value: saveSlot.Levels[2].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Dungeon of Murk",
                    value: saveSlot.Levels[4].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Bog of Murk",
                    value: saveSlot.Levels[5].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Rivers of Murk",
                    value: saveSlot.Levels[7].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Hoodlum Moor",
                    value: saveSlot.Levels[8].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Land of the Livid Dead",
                    value: saveSlot.Levels[9].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Menhirs of Power",
                    value: saveSlot.Levels[10].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Clouds of Peril",
                    value: saveSlot.Levels[12].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Heart of the World",
                    value: saveSlot.Levels[13].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Vertiginous Riddle",
                    value: saveSlot.Levels[15].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Cloudy Cache",
                    value: saveSlot.Levels[16].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Mélée Mayhem",
                    value: saveSlot.Levels[17].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Scalding Cascade",
                    value: saveSlot.Levels[18].Score),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RHR_Score,
                    header: "Sulphurous Sea",
                    value: saveSlot.Levels[19].Score),
            };

            yield return new EmulatedGameProgressionSlot(
                name: saveSlot.Name,
                index: saveIndex,
                collectiblesCount: lums + teensies + stamps + completedLevels,
                totalCollectiblesCount: totalLums + totalTeensies + totalStamps + totalLevels,
                emulatedSave: emulatedSave,
                dataItems: dataItems);
        }

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }

    // Extracted from the game data
    public LevelInfo[] LevelInfos { get; } = 
    {
        new(48, 4, 3000,  5000,  8000,  true,  false), // Fairy Council
        new(69, 4, 5000,  9000,  14000, true,  false), // Clearleaf Forest
        new(99, 4, 7000,  14000, 19000, true,  false), // Clearleaf Falls
        new(2,  0, 0,     0,     0,     false, false), // Infernal Machine
        new(75, 0, 5000,  11000, 16000, true,  false), // Dungeon of Murk
        new(76, 0, 11000, 18000, 26000, true,  false), // Bog of Murk
        new(4,  0, 0,     0,     0,     false, false), // Begoniax Bayou
        new(58, 4, 7000,  12000, 21000, true,  false), // Rivers of Murk
        new(96, 4, 10000, 22000, 30000, true,  false), // Hoodlum Moor
        new(85, 4, 8000,  16000, 23000, true,  false), // Land of the Livid Dead
        new(98, 4, 12000, 26000, 42000, true,  false), // Menhirs of Power
        new(0,  0, 0,     0,     0,     false, false), // Pit of Endless Fire
        new(87, 4, 12000, 26000, 42000, true,  false), // Clouds of Peril
        new(90, 4, 10000, 22000, 33000, true,  false), // Heart of the World
        new(6,  0, 0,     0,     0,     false, false), // Reflux's Lair
        new(82, 0, 12000, 18000, 28000, true,  true ), // Vertiginous Riddle
        new(99, 0, 12000, 18000, 28000, true,  true ), // Cloudy Cache
        new(92, 0, 11000, 16000, 26000, true,  true ), // Mélée Mayhem
        new(53, 0, 12000, 18000, 28000, true,  true ), // Scalding Cascade
        new(63, 0, 10000, 15000, 22000, true,  true ), // Sulphurous Sea
    };

    public record LevelInfo(int Lums, int Teensies, int StampScore1, int StampScore2, int StampScore3, bool HasScore, bool IsBonus);
}
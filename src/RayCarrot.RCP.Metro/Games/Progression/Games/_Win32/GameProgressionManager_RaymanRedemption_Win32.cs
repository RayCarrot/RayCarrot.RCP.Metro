using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanRedemption_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanRedemption_Win32(GameInstallation gameInstallation, string backupId) 
        : base(gameInstallation, backupId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // NOTE: Not currently localized due to the game not being localized
    protected string[] GetMagicianLevelNames => new string[]
    {
        "Swinging in the Jungle",
        "Vine-Shrine Hunt",
        "The Thorn Garden",
        "Maracas Madness",
        "Electoon's Octave",
        "Ring Toss Tussle",
        "Rock Hike",
        "The Cliffhanger",
        "Perilous Spikes",
        "Pencil Case Race",
        "Little Land",
        "Pinpoint Precision",
        "Getting Over It",
        "The Four Chambers",
        "Electric Meltdown",
        "The Short Trip",
        "The Train Tunnel",
        "Door to Door",
        "Sugar Rush",
        "Candy Climber",
        "The Final Slide",
        "Dark Magician's Challenge",
        "Boss Rush",
        "True Boss Rush",
    };

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "RaymanRedemption", SearchOption.AllDirectories, "*", "0", 0),
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath localAppData = Environment.SpecialFolder.LocalApplicationData.GetFolderPath();
        FileSystemPath? saveDir = fileSystem.GetDirectory(new IOSearchPattern(localAppData + "RaymanRedemption", SearchOption.TopDirectoryOnly, "*.txt"))?.DirPath;

        if (saveDir == null)
            yield break;

        // Potential other data to show:
        // levelTime{x} = 36 level times, 2 are unused
        // shop_item{x} = 34 shop items, but do minus 3 since they can be bought multiple times
        // joe_item{x} = 2 items, combine with shop items?
        // betillaupgrade - max is 4
        // healthmax - max is 7
        // Achievements (separate from the slots)

        using RCPContext context = new(saveDir);

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            string fileName = $"rayrede{saveIndex + 1}.txt";

            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, saveIndex);

            GameMaker_DSMap? saveData = await context.ReadFileDataAsync<GameMaker_DSMap>(fileName, new GameMaker_HexStringEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

            string saveName = saveData.GetValue("savename0").StringValue + 
                              saveData.GetValue("savename1").StringValue + 
                              saveData.GetValue("savename2").StringValue;

            string[] gameModes = { "Casual", "Classic", "Demise" };
            int gameMode = saveData.GetValue("gamemode").NumberValueAsInt;
            string gameModeStr = gameModes[gameMode];

            int getValuesCount(string key, string? invalidKey = null, int value = 1) => saveData.Entries.Count(x => 
                x.Key.StringValue.StartsWith(key) && 
                (invalidKey == null || !x.Key.StringValue.StartsWith(invalidKey)) && 
                x.Value.NumberValueAsInt == value);

            const int maxCages = 168;
            int cages = getValuesCount("cage");

            const int maxTokens = 40;
            int tokens = getValuesCount("token");

            const int maxPresents = 30;
            int presents = getValuesCount("present");

            const int maxRaymanSkins = 24;
            int raymanSkins = getValuesCount("unlock_skin");

            const int maxBzzitSkins = 5;
            int bzzitSkins = getValuesCount("unlock_moskito");

            const int maxCheckpointSkins = 13;
            int checkpointSkins = getValuesCount("unlock_checkpoint");

            const int maxLevelsCompleted = 37;
            int levelsCompleted = getValuesCount("levelcompleted");

            //const int maxMagicianLevelsCompleted = 24;
            int magicianLevelsCompleted = getValuesCount("magician", invalidKey: "magicianTime", value: 2);

            string lives = gameMode == 0 ? "∞" : saveData.GetValue("lives").NumberValueAsInt.ToString();

            int tings = saveData.GetValue("tings").NumberValueAsInt;

            // Get the magician level times
            string[] magicianLevelNames = GetMagicianLevelNames;
            IEnumerable<GameProgressionDataItem> magicianBonusDataItems = Enumerable.Range(0, 24).Select(i => new
            {
                Index = i,
                TimeValue = saveData.GetValue($"magicianTime{i}").NumberValue,
            }).Where(x => x.TimeValue > 0).Select(x =>
            {
                var icon = (ProgressionIconAsset)Enum.Parse(typeof(ProgressionIconAsset), $"Redemption_Magician{x.Index}");
                var time = TimeSpan.FromSeconds(x.TimeValue / 60);
                return new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: icon, 
                    header: magicianLevelNames[x.Index], 
                    text: $"{time:mm\\:ss\\.fff}");
            });

            int totalProgress = levelsCompleted +
                                tokens +
                                presents +
                                cages +
                                magicianLevelsCompleted +
                                getValuesCount("pink") +
                                saveData.GetValue("gamecompleted").NumberValueAsInt +
                                saveData.GetValue("betillaupgrade").NumberValueAsInt +
                                raymanSkins +
                                bzzitSkins +
                                checkpointSkins +
                                (saveData.GetValue("shop_item3").NumberValueAsInt == 2 ? 1 : 0) +
                                (saveData.GetValue("shop_item4").NumberValueAsInt == 2 ? 1 : 0) +
                                getValuesCount("joe_item");

            double percentage = Math.Floor(totalProgress / (double)352 * 100);

            var dataItems = new List<GameProgressionDataItem>()
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.R1_LevelExit, 
                    header: new ResourceLocString(nameof(Resources.Progression_LevelsCompleted)), 
                    value: levelsCompleted, max: maxLevelsCompleted),
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.R1_Cage,
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: cages, 
                    max: maxCages),
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.Redemption_Token, 
                    header: new ResourceLocString(nameof(Resources.Progression_RedemptionTokens)),
                    value: tokens, 
                    max: maxTokens),
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.Redemption_Present, 
                    header: new ResourceLocString(nameof(Resources.Progression_RedemptionPresents)),
                    value: presents, 
                    max: maxPresents),
                new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.Redemption_RaymanSkin, 
                    header: new ResourceLocString(nameof(Resources.Progression_RedemptionRaySkins)),
                    value: raymanSkins, 
                    max: maxRaymanSkins),
                new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.Redemption_BzzitSkin, 
                    header: new ResourceLocString(nameof(Resources.Progression_RedemptionBzzitSkins)),
                    value: bzzitSkins, 
                    max: maxBzzitSkins),
                new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.Redemption_CheckpointSkin, 
                    header: new ResourceLocString(nameof(Resources.Progression_RedemptionCheckpointSkins)),
                    value: checkpointSkins, 
                    max: maxCheckpointSkins),
                new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.R1_Ting, 
                    header: new ResourceLocString(nameof(Resources.Progression_Tings)),
                    value: tings),
                new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.R1_Life, 
                    header: new ResourceLocString(nameof(Resources.Progression_Lives)),
                    text: lives),
            };

            dataItems.AddRange(magicianBonusDataItems);

            yield return new SerializableGameProgressionSlot<GameMaker_DSMap>($"{saveName} ({gameModeStr})", saveIndex, percentage, dataItems, context, saveData, fileName);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}
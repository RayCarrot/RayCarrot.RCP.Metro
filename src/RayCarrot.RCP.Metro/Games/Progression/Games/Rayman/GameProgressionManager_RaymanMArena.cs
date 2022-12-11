using System.IO;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanMArena : GameProgressionManager
{
    public GameProgressionManager_RaymanMArena(GameInstallation gameInstallation, string backupName, bool isRaymanMDemo) 
        : base(gameInstallation, backupName)
    {
        IsRaymanMDemo = isRaymanMDemo;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool IsRaymanMDemo { get; }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation + "MENU" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        // Get the save data directory
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "MENU" + "SaveGame", SearchOption.TopDirectoryOnly, "*.sav"));

        if (saveDir == null)
            yield break;

        FileSystemPath saveFileName = "raymanm.sav";

        Logger.Info("{0} save file {1} is being loaded...", GameInstallation.FullId, saveFileName);

        using RCPContext context = new(saveDir.DirPath);

        // Deserialize the save data
        RMSaveFile? saveData = await context.ReadFileDataAsync<RMSaveFile>(saveFileName, removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("Save file has been deserialized");

        // Helper for getting the entry from a specific key
        IEnumerable<int> GetValues(string key) => saveData.Items.First(x => x.Key == key).Values.Select(x => x.IntValue);

        // Helper for getting the time from an integer
        static TimeSpan GetTime(int value)
        {
            var milliSeconds = value % 1000;
            var calc2_1 = (value - milliSeconds) / 1000;
            var seconds = calc2_1 % 60;
            var minute = (calc2_1 - seconds) / 60;

            return new TimeSpan(0, 0, minute, seconds, milliSeconds);
        }

        // There is a max of 8 save slots in every version
        for (int slotIndex = 0; slotIndex < 8; slotIndex++)
        {
            // Get the save name
            string name = saveData.Items.First(x => x.Key == "sg_names").Values[slotIndex].StringValue;

            // Make sure it's valid
            if (name.Contains("No Data"))
                continue;

            // Create the collection with items
            List<GameProgressionDataItem> progressItems = new();

            // NOTE:
            // The race level modes get a bit confusing in the bonus league. There are 12 normal races and the bonus league
            // has 5 additional ones. However they seem to handle the values in the save in an odd way, with some levels
            // getting set to 2 (completed) even when they only have a training mode. Since I'm unsure what the true max
            // would be I've opted out for not counting them and then manually adding mode 1 for the two bonus levels
            // with an actual completable mode. Hopefully this will be accurate.

            // Get completed challenges
            int raceCompleted = 0;
            int battleCompleted = 0;
            int maxRace = IsRaymanMDemo ? 15 : 38;
            int maxBattle = IsRaymanMDemo ? 15 : 13 * 3;

            void AddRaceCompleted(IEnumerable<int> values, int count) => 
                raceCompleted += values.Skip(count * slotIndex).Take(12).Count(x => x == 2);
            void AddBattleCompleted(IEnumerable<int> values) => 
                battleCompleted += values.Skip(13 * slotIndex).Take(13).Count(x => x == 2);

            AddRaceCompleted(GetValues("sg_racelevels_mode1"), 17);
            AddRaceCompleted(GetValues("sg_racelevels_mode2"), 16);
            AddRaceCompleted(GetValues("sg_racelevels_mode3"), 16);

            // Unoptimized code for adding Speed Stress & On and On
            raceCompleted += GetValues("sg_racelevels_mode1").Skip(17 * slotIndex + 14).Take(1).Count(x => x == 2);
            raceCompleted += GetValues("sg_racelevels_mode1").Skip(17 * slotIndex + 16).Take(1).Count(x => x == 2);

            AddBattleCompleted(GetValues("sg_battlelevels_mode1"));
            AddBattleCompleted(GetValues("sg_battlelevels_mode2"));
            AddBattleCompleted(GetValues("sg_battlelevels_mode3"));

            // Add completed challenges
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: ProgressionIconAsset.RM_Race,
                header: new ResourceLocString(nameof(Resources.Progression_RMRacesCompleted)),
                value: raceCompleted, 
                max: maxRace));
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: ProgressionIconAsset.RM_Battle,
                header: new ResourceLocString(nameof(Resources.Progression_RMBattlesCompleted)),
                value: battleCompleted, 
                max: maxBattle));

            // Add records for every race
            for (int raceIndex = 0; raceIndex < 16; raceIndex++)
            {
                // Get the index to use for this race in this slot
                int index = slotIndex * 16 + raceIndex;

                // Helper method for adding a race item
                void AddRaceItem(string key, Func<string> getDescription, bool isTime = true)
                {
                    // Get the value
                    var value = GetValues(key).ElementAt(index);

                    // Only add if it has valid data
                    if ((isTime && value > 0) || (!isTime && value != -22))
                        progressItems.Add(new GameProgressionDataItem(
                            isPrimaryItem: false,
                            // Get the level icon
                            icon: Enum.Parse(typeof(ProgressionIconAsset), $"RM_R{raceIndex}").CastTo<ProgressionIconAsset>(),
                            // The header
                            header: new GeneratedLocString(() => $"{Resources.ResourceManager.GetString($"RM_RaceName_{raceIndex}", Services.InstanceData.CurrentCulture)} ({getDescription()})"),
                            // The value
                            text: new GeneratedLocString(() => $"{(isTime ? GetTime(value).ToString("mm\\:ss\\.fff") : value.ToString())}")));
                }

                AddRaceItem("sg_racelevels_bestlap_training", () => Resources.Progression_RM_LapTraining);
                AddRaceItem("sg_racelevels_bestlap_race", () => Resources.Progression_RM_LapRace);
                AddRaceItem("sg_racelevels_bestlap_target", () => Resources.Progression_RM_LapTarget);
                AddRaceItem("sg_racelevels_bestlap_lums", () => Resources.Progression_RM_LapLums);
                AddRaceItem("sg_racelevels_besttime_race", () => Resources.Progression_RM_TimeRace);
                AddRaceItem("sg_racelevels_besttime_target", () => Resources.Progression_RM_TimeTarget);
                AddRaceItem("sg_racelevels_besttime_lums", () => Resources.Progression_RM_TimeLums);
                AddRaceItem("sg_racelevels_bestnumber_target", () => Resources.Progression_RM_Targets, false);
                AddRaceItem("sg_racelevels_bestnumber_lums", () => Resources.Progression_RM_Lums, false);
            }

            yield return new SerializableGameProgressionSlot<RMSaveFile>(name.TrimEnd(), slotIndex, raceCompleted + battleCompleted, maxRace + maxBattle, progressItems, context, saveData, saveFileName);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}
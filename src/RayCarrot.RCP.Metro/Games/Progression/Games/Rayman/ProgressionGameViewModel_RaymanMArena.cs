using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanMArena : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanMArena(Games game) : base(game) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir() + "Menu" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        // Get the save data directory
        FileSystemPath saveDir = InstallDir + "MENU" + "SaveGame";
        FileSystemPath filePath = saveDir + "raymanm.sav";

        Logger.Info("{0} save file {1} is being loaded...", Game, filePath.Name);

        // Deserialize the save data
        OpenSpaceSettings settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.RaymanM, Platform.PC);
        (RaymanMPCSaveData? saveData, filePath) = await SerializeFileDataAsync<RaymanMPCSaveData>(fileSystem, filePath, settings);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", Game);
            yield break;
        }

        Logger.Info("Save file has been deserialized");

        // Helper for getting the entry from a specific key
        int[] GetValues(string key) => saveData.Items.First(x => x.Key == key).Values;

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
            string name = saveData.Items.First(x => x.Key == "sg_names").StringValues[slotIndex];

            // Make sure it's valid
            if (name.Contains("No Data"))
                continue;

            // Create the collection with items
            List<ProgressionDataViewModel> progressItems = new();

            // Get completed challenges
            int raceCompleted = 0;
            int battleCompleted = 0;
            int maxRace = Game == Games.Demo_RaymanM ? 15 : 40;
            int maxBattle = Game == Games.Demo_RaymanM ? 15 : 13 * 3;

            void AddRaceCompleted(IEnumerable<int> values) => raceCompleted += values.Skip(17 * slotIndex).Take(17).Count(x => x == 2);
            void AddBattleCompleted(IEnumerable<int> values) => battleCompleted += values.Skip(13 * slotIndex).Take(13).Count(x => x == 2);

            AddRaceCompleted(GetValues("sg_racelevels_mode1"));
            AddRaceCompleted(GetValues("sg_racelevels_mode2"));
            AddRaceCompleted(GetValues("sg_racelevels_mode3"));
            AddBattleCompleted(GetValues("sg_battlelevels_mode1"));
            AddBattleCompleted(GetValues("sg_battlelevels_mode2"));
            AddBattleCompleted(GetValues("sg_battlelevels_mode3"));

            // Add completed challenges
            // TODO-UPDATE: Localize
            progressItems.Add(new ProgressionDataViewModel(true, ProgressionIcon.RM_Race, new ConstLocString("Races finished"), raceCompleted, maxRace));
            progressItems.Add(new ProgressionDataViewModel(true, ProgressionIcon.RM_Battle, new ConstLocString("Battles finished"), battleCompleted, maxBattle));

            // Add records for every race
            for (int raceIndex = 0; raceIndex < 16; raceIndex++)
            {
                // Get the index to use for this race in this slot
                int index = slotIndex * 16 + raceIndex;

                // Helper method for adding a race item
                void AddRaceItem(string key, Func<string> getDescription, bool isTime = true)
                {
                    // Get the value
                    var value = GetValues(key)[index];

                    // Only add if it has valid data
                    if ((isTime && value > 0) || (!isTime && value > -22))
                        // TODO-UPDATE: Update localization for the get description
                        progressItems.Add(new ProgressionDataViewModel(
                            isPrimaryItem: false,
                            // Get the level icon
                            icon: Enum.Parse(typeof(ProgressionIcon), $"RM_R{raceIndex}").CastTo<ProgressionIcon>(),
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

            yield return new SerializableProgressionSlotViewModel<RaymanMPCSaveData>(this, new ConstLocString(name.TrimEnd()), slotIndex, raceCompleted + battleCompleted, maxRace + maxBattle, progressItems, saveData, settings)
            {
                FilePath = filePath
            };

            Logger.Info("{0} slot has been loaded", Game);
        }
    }
}
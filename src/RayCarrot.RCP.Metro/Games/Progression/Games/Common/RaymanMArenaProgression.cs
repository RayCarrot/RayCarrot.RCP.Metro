using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public static class RaymanMArenaProgression
{
    public static IReadOnlyList<GameProgressionDataItem> CreateProgressionItems(
        RMSaveFile saveFile, 
        bool isDemo,
        int slotIndex,
        out int collectiblesCount, 
        out int maxCollectiblesCount)
    {
        // Helper for getting the entry from a specific key
        IEnumerable<uint> GetValues(string key) => saveFile.Elements.First(x => x.ElementName == key).Values.Select(x => x.IntegerValue);

        // Helper for getting the time from an integer
        static TimeSpan GetTime(uint value)
        {
            uint milliSeconds = value % 1000;
            uint calc2_1 = (value - milliSeconds) / 1000;
            uint seconds = calc2_1 % 60;
            uint minute = (calc2_1 - seconds) / 60;

            return new TimeSpan(0, 0, (int)minute, (int)seconds, (int)milliSeconds);
        }

        // Create the collection with items
        List<GameProgressionDataItem> dataItems = new();

        // NOTE:
        // The race level modes get a bit confusing in the bonus league. There are 12 normal races and the bonus league
        // has 5 additional ones. However they seem to handle the values in the save in an odd way, with some levels
        // getting set to 2 (completed) even when they only have a training mode. Since I'm unsure what the true max
        // would be I've opted out for not counting them and then manually adding mode 1 for the two bonus levels
        // with an actual completable mode. Hopefully this will be accurate.

        // Get completed challenges
        int raceCompleted = 0;
        int battleCompleted = 0;
        int maxRace = isDemo ? 15 : 38;
        int maxBattle = isDemo ? 15 : 13 * 3;

        void AddRaceCompleted(IEnumerable<uint> values, int count) =>
            raceCompleted += values.Skip(count * slotIndex).Take(12).Count(x => x == 2);
        void AddBattleCompleted(IEnumerable<uint> values) =>
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
        dataItems.Add(new GameProgressionDataItem(
            isPrimaryItem: true,
            icon: ProgressionIconAsset.RM_Race,
            header: new ResourceLocString(nameof(Resources.Progression_RMRacesCompleted)),
            value: raceCompleted,
            max: maxRace));
        dataItems.Add(new GameProgressionDataItem(
            isPrimaryItem: true,
            icon: ProgressionIconAsset.RM_Battle,
            header: new ResourceLocString(nameof(Resources.Progression_RMBattlesCompleted)),
            value: battleCompleted,
            max: maxBattle));

        // Add records for every race
        for (int i = 0; i < 16; i++)
        {
            // Local copy to be captured correctly by the inner function
            int raceIndex = i;

            // Get the index to use for this race in this slot
            int index = slotIndex * 16 + raceIndex;

            // Helper method for adding a race item
            void AddRaceItem(string key, Func<string> getDescription, bool isTime = true)
            {
                // Get the value
                var value = GetValues(key).ElementAt(index);

                // Only add if it has valid data
                if (value != 0xFFFFFFEA)
                    dataItems.Add(new GameProgressionDataItem(
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

        collectiblesCount = raceCompleted + battleCompleted;
        maxCollectiblesCount = maxRace + maxBattle;

        return dataItems;
    }
}
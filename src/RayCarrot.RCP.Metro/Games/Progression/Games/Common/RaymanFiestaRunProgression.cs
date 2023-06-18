using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro
{
    public static class RaymanFiestaRunProgression
    {
        private static int GetLevelIdFromIndex(int idx)
        {
            int v2 = idx + 1;
            if (v2 % 10 == 0)
                return v2 / 10 + 36;

            int v3 = idx % 100;
            if (idx % 100 > 8)
            {
                if (v3 > 18)
                {
                    if (v3 <= 28)
                        return idx - 1;
                    if (v3 <= 38)
                        return idx - 2;
                }
                v2 = idx;
            }
            return v2;
        }

        public static List<GameProgressionDataItem> CreateProgressionItems(
            FiestaRun_SaveData saveData, 
            out int collectiblesCount, 
            out int maxCollectiblesCount)
        {
            int crowns = saveData.LevelInfos_Land1.Count(x => x.HasCrown);
            int maxCrowns = 72;

            List<GameProgressionDataItem> progressItems = new();

            if (saveData.Version >= 2)
            {
                crowns += saveData.LevelInfos_Land2.Count(x => x.HasCrown);
                maxCrowns += 16;
            }

            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.RFR_Crown,
                header: new ResourceLocString(nameof(Resources.Progression_RFRCrowns)),
                value: crowns,
                max: maxCrowns));

            if (saveData.Version >= 2)
                progressItems.Add(new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.RFR_Nightmare,
                    header: new ResourceLocString(nameof(Resources.Progression_RFRNightmareMode)),
                    value: GetLevelIdFromIndex(saveData.MaxNightMareLevelIdx % 100),
                    max: 36));

            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.RL_Lum,
                header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                value: (int)saveData.LumsGlobalCounter));

            // Add Livid Dead times
            for (int lvlIndex = 0; lvlIndex < saveData.LevelTimes.Length; lvlIndex++)
            {
                if (saveData.LevelTimes[lvlIndex] == 0)
                    continue;

                // Add the item
                progressItems.Add(new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.RO_Clock,
                    header: new ResourceLocString($"RFR_LevelName_{lvlIndex + 1}_10"),
                    text: $"{new TimeSpan(0, 0, 0, 0, (int)saveData.LevelTimes[lvlIndex]):mm\\:ss\\.fff}"));
            }

            collectiblesCount = crowns;
            maxCollectiblesCount = maxCrowns;

            return progressItems;
        }
    }
}
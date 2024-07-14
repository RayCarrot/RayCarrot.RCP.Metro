using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    public static class Rayman3Progression
    {
        public static IReadOnlyList<GameProgressionDataItem> CreateProgressionItems(
            R3SaveFile saveFile, 
            out int collectiblesCount, 
            out int maxCollectiblesCount)
        {
            R3EnvironmentValues envValues = saveFile.SaveList.EnvironmentValues;

            int[] stampScores = { 20820, 44500, 25900, 58000, 55500, 26888, 26700, 43700, 48000 };

            int stamps = stampScores.Where((stampScore, i) => stampScore < envValues.Levels[i].Score).Count();

            collectiblesCount = envValues.Total.Cages + stamps;
            maxCollectiblesCount = 60 + stampScores.Length;

            // Create the collection with items for each level + general information
            return new GameProgressionDataItem[]
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R3_Cage,
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: envValues.Total.Cages,
                    max: 60),
                new GameProgressionDataItem(
                    isPrimaryItem: true,
                    icon: ProgressionIconAsset.R3_Stamp,
                    header: new ResourceLocString(nameof(Resources.Progression_R3Stamps)),
                    value: stamps,
                    max: stampScores.Length),
                new GameProgressionDataItem(
                    isPrimaryItem: false,
                    icon: ProgressionIconAsset.R3_Score,
                    header: new ResourceLocString(nameof(Resources.Progression_TotalScore)),
                    value: envValues.Total.Score),

                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level1Header)), envValues.Levels[0].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level2Header)), envValues.Levels[1].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level3Header)), envValues.Levels[2].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level4Header)), envValues.Levels[3].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level5Header)), envValues.Levels[4].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level6Header)), envValues.Levels[5].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level7Header)), envValues.Levels[6].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level8Header)), envValues.Levels[7].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level9Header)), envValues.Levels[8].Score),
            };
        }
    }
}
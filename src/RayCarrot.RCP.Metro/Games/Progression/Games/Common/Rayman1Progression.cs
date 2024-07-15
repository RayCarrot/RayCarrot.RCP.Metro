using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public static class Rayman1Progression
{
    public static IReadOnlyList<GameProgressionDataItem> CreateProgressionItems(
        SaveSlot saveSlot, 
        out int collectiblesCount, 
        out int maxCollectiblesCount)
    {
        // Get total amount of cages
        int cages = saveSlot.WorldInfoSaveZone.Sum(x => x.Cages);

        GameProgressionDataItem[] dataItems =
        {
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.R1_Cage,
                header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                value: cages,
                max: 102),
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R1_Continue,
                header: new ResourceLocString(nameof(Resources.Progression_Continues)),
                value: saveSlot.ContinuesCount),
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R1_Life,
                header: new ResourceLocString(nameof(Resources.Progression_Lives)),
                value: saveSlot.StatusBar.LivesCount),
        };

        collectiblesCount = cages;
        maxCollectiblesCount = 102;

        return dataItems;
    }
}
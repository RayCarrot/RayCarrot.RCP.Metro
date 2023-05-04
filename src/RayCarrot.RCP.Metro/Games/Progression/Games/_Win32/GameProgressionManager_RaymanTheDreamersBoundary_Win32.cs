using System.IO;
using Newtonsoft.Json.Linq;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanTheDreamersBoundary_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanTheDreamersBoundary_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman_The_Dreamer_s_Boundary", SearchOption.AllDirectories, "*", "0", 0),
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath localAppData = Environment.SpecialFolder.LocalApplicationData.GetFolderPath();
        IOSearchPattern? dirSearchPattern = fileSystem.GetDirectory(
            new IOSearchPattern(localAppData + "Rayman_The_Dreamer_s_Boundary", SearchOption.TopDirectoryOnly, "*.sav"));

        if (dirSearchPattern == null)
            yield break;

        FileSystemPath saveDir = dirSearchPattern.DirPath;

        for (int saveIndex = 0; saveIndex < 2; saveIndex++)
        {
            FileSystemPath filePath = saveDir + $"savegame{saveIndex + 1}.sav";

            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, saveIndex);

            if (!filePath.FileExists)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            JObject json = await Task.Run(() => JObject.Parse(File.ReadAllText(filePath)));

            Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

            // With 1.2 there are 38 levels, but they're treated as bonus completion beyond 100%
            const int totalLevels = 36;
            int completedLevels = json["ROOT"]?[0]?.Value<int>("Levels Completed") ?? 0;

            // With 1.2 there are 112 levels, but they're treated as bonus completion beyond 100%
            const int totalCages = 108;
            int collectedCages = json["ROOT"]?[0]?.Value<int>("Cages Collected") ?? 0;

            double percentage = (collectedCages + completedLevels) / 144.0 * 100.0;

            if (completedLevels == 38 && collectedCages == 112)
                percentage = 105;

            var dataItems = new List<GameProgressionDataItem>()
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.RTDB_LevelComplete, 
                    header: new ResourceLocString(nameof(Resources.Progression_LevelsCompleted)), 
                    value: completedLevels, max: totalLevels),
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.RTDB_Cage, 
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)), 
                    value: collectedCages, max: totalCages),
            };

            yield return new GameProgressionSlot(null, saveIndex, percentage, filePath, dataItems);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}
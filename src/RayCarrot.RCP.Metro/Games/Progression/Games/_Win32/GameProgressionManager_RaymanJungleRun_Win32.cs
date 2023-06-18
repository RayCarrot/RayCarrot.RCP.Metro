using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanJungleRun_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanJungleRun_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(UbisoftConnectHelpers.GetSaveDirectory(GameInstallation), SearchOption.TopDirectoryOnly, "*.save", "0", 0),
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath dirPath = UbisoftConnectHelpers.GetSaveDirectory(GameInstallation);
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(dirPath, SearchOption.TopDirectoryOnly, "*.save"));

        if (saveDir is null)
            yield break;

        using RCPContext context = new(saveDir.DirPath);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanJungleRun, Platform.PC);
        context.AddSettings(settings);

        Logger.Info("{0} slot is being loaded...", GameInstallation.FullId);

        // Get the file path
        const string fileName = "ROscores.save";

        // Deserialize the data
        UPC_StorageFile<JungleRun_SaveData>? storageFile = await context.ReadFileDataAsync<UPC_StorageFile<JungleRun_SaveData>>(fileName, endian: Endian.Little, removeFileWhenComplete: false);

        if (storageFile == null)
        {
            Logger.Info("{0} slot was not found", GameInstallation.FullId);
            yield break;
        }

        JungleRun_SaveData saveData = storageFile.Content;

        Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

        // Create the collection with items for each time trial level + general information
        List<GameProgressionDataItem> progressItems = new();

        const int numWorlds = 7;
        const int maxLums = numWorlds * 9 * 100;
        const int maxTeeth = numWorlds * 10;

        int lums = 0;
        int teeth = 0;

        // Enumerate each level
        for (int lvl = 0; lvl < saveData.LevelInfos.Length; lvl++)
        {
            // Get the level data
            JungleRun_SaveDataLevel levelData = saveData.LevelInfos[lvl];

            // Check if the level is a normal level
            if ((lvl + 1) % 10 != 0)
            {
                // Get the collected lums
                lums += levelData.LumsRecord;

                // Check if the level is 100% complete
                if (levelData.LumsRecord >= 100)
                    teeth++;

                continue;
            }

            // Make sure the level has been completed
            if (levelData.RecordTime == 0)
                continue;

            teeth++;

            // Get the level number, starting at 10
            string fullLevelNumber = (lvl + 11).ToString();

            // Get the world and level numbers
            string worldNum = fullLevelNumber[0].ToString();
            string lvlNum = fullLevelNumber[1].ToString();

            // If the level is 0, correct the numbers to be level 10
            if (lvlNum == "0")
            {
                worldNum = (Int32.Parse(worldNum) - 1).ToString();
                lvlNum = "10";
            }

            // Add the item
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.RO_Clock,
                header: $"{worldNum}-{lvlNum}",
                text: $"{new TimeSpan(0, 0, 0, 0, (int)levelData.RecordTime):mm\\:ss\\.fff}"));
        }

        // Add general progress info first
        progressItems.Insert(0, new GameProgressionDataItem(
            isPrimaryItem: true,
            icon: ProgressionIconAsset.RO_Lum,
            header: new ResourceLocString(nameof(Resources.Progression_Lums)),
            value: lums,
            max: maxLums));
        progressItems.Insert(1, new GameProgressionDataItem(
            isPrimaryItem: true,
            icon: ProgressionIconAsset.RO_RedTooth,
            header: new ResourceLocString(nameof(Resources.Progression_Teeth)),
            value: teeth,
            max: maxTeeth));

        yield return new SerializableGameProgressionSlot<UPC_StorageFile<JungleRun_SaveData>>(null, 0, lums + teeth, maxLums + maxTeeth, progressItems, context, storageFile, fileName);

        Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
    }
}
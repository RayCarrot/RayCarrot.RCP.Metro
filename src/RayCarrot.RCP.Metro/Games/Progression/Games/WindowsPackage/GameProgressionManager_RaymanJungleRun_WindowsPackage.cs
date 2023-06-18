using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanJungleRun_WindowsPackage : GameProgressionManager
{
    public GameProgressionManager_RaymanJungleRun_WindowsPackage(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId)
    {
        GameDescriptor = gameDescriptor;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private WindowsPackageGameDescriptor GameDescriptor { get; }
    public override GameBackups_Directory[] BackupDirectories => GameDescriptor.GetBackupDirectories();

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath dirPath = GameDescriptor.GetLocalAppDataDirectory();
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(dirPath, SearchOption.TopDirectoryOnly, "*.dat"));

        if (saveDir is null)
            yield break;

        using RCPContext context = new(saveDir.DirPath);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanJungleRun, Platform.PC);
        context.AddSettings(settings);

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, saveIndex);

            // Get the file path
            string fileName = $"slot{saveIndex + 1}.dat";

            // Deserialize the data
            JungleRun_SaveData? saveData = await context.ReadFileDataAsync<JungleRun_SaveData>(fileName, endian: Endian.Little, removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

            // Create the collection with items for each time trial level + general information
            IReadOnlyList<GameProgressionDataItem> progressItems = RaymanJungleRunProgression.CreateProgressionItems(
                saveData, out int collectiblesCount, out int maxCollectiblesCount);

            yield return new SerializableGameProgressionSlot<JungleRun_SaveData>(null, saveIndex, collectiblesCount, maxCollectiblesCount, progressItems, context, saveData, fileName);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}
using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanFiestaRun_WindowsPackage : GameProgressionManager
{
    public GameProgressionManager_RaymanFiestaRun_WindowsPackage(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation, string progressionId, int slotIndex) 
        : base(gameInstallation, progressionId)
    {
        GameDescriptor = gameDescriptor;
        SlotIndex = slotIndex;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private int SlotIndex { get; }
    private WindowsPackageGameDescriptor GameDescriptor { get; }

    public override GameBackups_Directory[] BackupDirectories => GameDescriptor.GetBackupDirectories();

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath dirPath = GameDescriptor.GetLocalAppDataDirectory();
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(dirPath, SearchOption.TopDirectoryOnly, "*.dat"));

        if (saveDir is null)
            yield break;

        using RCPContext context = new(saveDir.DirPath);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC);
        context.AddSettings(settings);

        Logger.Info("{0} slot is being loaded...", GameInstallation.FullId);

        // Get the file path
        string fileName = $"slot{SlotIndex}.dat";

        // Deserialize the data
        FiestaRun_SaveData? saveData = await context.ReadFileDataAsync<FiestaRun_SaveData>(fileName, endian: Endian.Little, removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} slot was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

        IReadOnlyList<GameProgressionDataItem> progressItems = RaymanFiestaRunProgression.CreateProgressionItems(
            saveData, out int collectiblesCount, out int maxCollectiblesCount);

        yield return new SerializableGameProgressionSlot<FiestaRun_SaveData>(null, 0, collectiblesCount, maxCollectiblesCount, progressItems, context, saveData, fileName);

        Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
    }
}
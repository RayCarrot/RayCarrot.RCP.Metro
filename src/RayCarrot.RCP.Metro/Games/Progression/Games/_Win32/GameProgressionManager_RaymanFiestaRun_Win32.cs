using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanFiestaRun_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanFiestaRun_Win32(GameInstallation gameInstallation, string progressionId) 
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
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC);
        context.AddSettings(settings);

        Logger.Info("{0} slot is being loaded...", GameInstallation.FullId);

        // Get the file path
        const string fileName = "RFRscores.save";

        // Deserialize the data
        UPC_StorageFile<FiestaRun_SaveData>? storageFile = await context.ReadFileDataAsync<UPC_StorageFile<FiestaRun_SaveData>>(fileName, endian: Endian.Little, removeFileWhenComplete: false);

        if (storageFile == null)
        {
            Logger.Info("{0} slot was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

        List<GameProgressionDataItem> progressItems = RaymanFiestaRunProgression.CreateProgressionItems(
            storageFile.Content, out int collectiblesCount, out int maxCollectiblesCount);

        yield return new SerializableGameProgressionSlot<UPC_StorageFile<FiestaRun_SaveData>>(null, 0, collectiblesCount, maxCollectiblesCount, progressItems, context, storageFile, fileName);

        Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
    }
}
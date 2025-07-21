using BinarySerializer;
using BinarySerializer.Ray1;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanRavingRabbids2_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanRavingRabbids2_Win32(GameInstallation gameInstallation, string progressionId)
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "RRR2", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };
    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveFile = fileSystem.GetFile(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "RRR2" + "RRR2.sav");

        Logger.Info("{0} save is being loaded...", GameInstallation.FullId);

        using RCPContext context = new(saveFile.Parent);

        RRR2_SaveFile? saveData = await context.ReadFileDataAsync<RRR2_SaveFile>(saveFile.Name, new RRR_SaveEncoder(), removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("Save has been deserialized");

        List<GameProgressionDataItem> progressItems = new();

        progressItems.AddRange(Enumerable.Range(0, 16).
        Select(x => new GameProgressionDataItem(
        isPrimaryItem: false,
        icon: ProgressionIconAsset.RHR_Score,
        header: "Bla-Bla Cafe",
        value: saveData.MiniGames[x].Scores[0].Score)));

        yield return new SerializableGameProgressionSlot<RRR2_SaveFile>(
    name: "Slot 1",
    index: 0,
    collectiblesCount: 0,
    totalCollectiblesCount: 16,
    dataItems: progressItems,
    context: context,
    serializable: saveData,
    fileName: saveFile.Name);

    }
}
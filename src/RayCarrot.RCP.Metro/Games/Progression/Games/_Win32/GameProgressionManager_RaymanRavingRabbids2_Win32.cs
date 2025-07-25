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
        FileSystemPath? saveDir = fileSystem.GetFile(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "RRR2");
        if (saveDir == null)
        {
            Logger.Info("{0} save directory was not found", GameInstallation.FullId);
            yield break;
        }

        for (int gameIndex = 0; gameIndex < GameClasses.Length; gameIndex++)
        {
            FileSystemPath saveFile = fileSystem.GetFile(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "RRR2" + "RRR2.sav");

            using RCPContext context = new(saveDir);

            RRR2_SaveFile? saveData = await context.ReadFileDataAsync<RRR2_SaveFile>(GameClasses[gameIndex].SaveFileName, new RRR_SaveEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                yield break;
            }

            Logger.Info("{0} save {1} has been deserialized...", GameInstallation.FullId, GameClasses[gameIndex].SaveFileName);

            int completedLevels = saveData.MiniGames.Count(x => (x.UserHighScore > 0));
            List<GameProgressionDataItem> progressItems = new()
        {
            new GameProgressionDataItem(
            isPrimaryItem: true,
            icon: ProgressionIconAsset.RRR_Trophy,
            header: new ResourceLocString(nameof(Resources.Progression_LevelsCompleted)),
            value: completedLevels,
            max: 16),
        };

            progressItems.AddRange(Enumerable.Range(0, 16).
                    Where(x => saveData.MiniGames[x].UserHighScore > 0).
                    Select(x => new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.RRR_Star,
                        header: new ResourceLocString($"RRR2_LevelName_{x}"),
                        value: saveData.MiniGames[x].UserHighScore)));

            yield return new SerializableGameProgressionSlot<RRR2_SaveFile>(
                name: GameClasses[gameIndex].GameDescription,
                index: 0,
                collectiblesCount: 0,
                totalCollectiblesCount: 16,
                dataItems: progressItems,
                context: context,
                serializable: saveData,
                fileName: saveFile.Name);
        }
    }

    public GameClass[] GameClasses { get; } =
    {
        new("RRR2.sav", "Allgames"),
        new("RRR2_Blue.sav", "Blue"),
        new("RRR2_Green.sav", "Green"),
        new("RRR2_Red.sav", "Red"),
        new("RRR2_Orange.sav", "Orange"),
    };
    public record GameClass(string SaveFileName, string GameDescription);
}
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
            using RCPContext context = new(saveDir);

            RRR2_SaveFile? saveData = await context.ReadFileDataAsync<RRR2_SaveFile>(GameClasses[gameIndex].SaveFileName, endian: Endian.Little, removeFileWhenComplete: false);

            if (saveData == null)
            {
                continue;
            }

            Logger.Info("{0} save {1} has been deserialized...", GameInstallation.FullId, GameClasses[gameIndex].SaveFileName);

            int[] userHighScores = new int[16];
            int completedLevels = 0;

            for (int i = 0; i < userHighScores.Length; i++)
            {
                RRR2_MiniGame game = saveData.MiniGames[i];
                if (game.Scores[0].Score != 12000 || game.Scores[0].EncodedName != EncodedName_Globox)
                {
                    userHighScores[i] = game.Scores[0].Score;
                    completedLevels++;
                }
                else if (game.Scores[1].Score != 8000 || game.Scores[1].EncodedName != EncodedName_Betilla)
                {
                    userHighScores[i] = game.Scores[1].Score;
                    completedLevels++;
                }
                else if (game.Scores[2].Score != 4000 || game.Scores[2].EncodedName != EncodedName_Murfy)
                {
                    userHighScores[i] = game.Scores[2].Score;
                    completedLevels++;
                }
                else
                {
                    userHighScores[i] = 0;
                }
            }

            List<GameProgressionDataItem> progressItems = new()
            {
                new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.RRR2_Trophy,
                header: new ResourceLocString(nameof(Resources.Progression_LevelsCompleted)),
                value: completedLevels,
                max: GameClasses[gameIndex].NumLevels),
            };

            progressItems.AddRange(Enumerable.Range(GameClasses[gameIndex].FirstLevelIndex, GameClasses[gameIndex].NumLevels).
                    Where(x => userHighScores[x] > 0).
                    Select(x => new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ((userHighScores[x] >= 12000) ? ProgressionIconAsset.RRR2_Medal_1 :
                               (userHighScores[x] >= 8000) ?  ProgressionIconAsset.RRR2_Medal_2 :
                                                              ProgressionIconAsset.RRR2_Medal_3),
                        header: new ResourceLocString($"RRR2_LevelName_{x}"),
                        value: userHighScores[x])));

            yield return new SerializableGameProgressionSlot<RRR2_SaveFile>(
                name: GameClasses[gameIndex].GameDescription,
                index: 0,
                collectiblesCount: completedLevels,
                totalCollectiblesCount: GameClasses[gameIndex].NumLevels,
                dataItems: progressItems,
                context: context,
                serializable: saveData,
                fileName: GameClasses[gameIndex].SaveFileName);
        }
    }

    public GameClass[] GameClasses { get; } =
    {
        new("RRR2.sav", "Allgames", 16, 0),
        new("RRR2_Blue.sav", "Blue", 4, 0),
        new("RRR2_Green.sav", "Green", 4, 4),
        new("RRR2_Red.sav", "Red", 4, 8),
        new("RRR2_Orange.sav", "Orange", 4, 12),
    };
    public record GameClass(string SaveFileName, string GameDescription, int NumLevels, int FirstLevelIndex);

    public const ulong EncodedName_Globox = 0x4F580000474C4F42;
    public const ulong EncodedName_Betilla = 0x4C4C410042455449;
    public const ulong EncodedName_Murfy = 0x590000004D555246;
}
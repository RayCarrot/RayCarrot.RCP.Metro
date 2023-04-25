namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RabbidsBigBang_WindowsPackage : GameProgressionManager
{
    public GameProgressionManager_RabbidsBigBang_WindowsPackage(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId)
    {
        GameDescriptor = gameDescriptor;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private WindowsPackageGameDescriptor GameDescriptor { get; }

    public override GameBackups_Directory[] BackupDirectories => GameDescriptor.GetBackupDirectories();

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveFile = fileSystem.GetFile(GameDescriptor.GetLocalAppDataDirectory() + "playerprefs.dat");

        using RCPContext context = new(saveFile.Parent);

        Logger.Info("{0} save is being loaded...", GameInstallation.FullId);

        Unity_PlayerPrefs? saveData = await context.ReadFileDataAsync<Unity_PlayerPrefs>(saveFile.Name, removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("{0} save has been deserialized", GameInstallation.FullId);

        const int maxScore = 12 * 45 * 3;

        int score = 0;

        for (int worldIndex = 0; worldIndex < 12; worldIndex++)
        {
            for (int levelIndex = 0; levelIndex < 45; levelIndex++)
            {
                Unity_PlayerPrefsEntry? entry = saveData.Entries.FirstOrDefault(x => x.Key == $"MissionComplete_{worldIndex + 1}_{levelIndex}");

                if (entry != null)
                    score += entry.IntValue;
            }
        }

        GameProgressionDataItem[] progressItems =
        {
            new GameProgressionDataItem(
                isPrimaryItem: true, 
                icon: ProgressionIconAsset.RabbidsBigBang_Score, 
                header: new ResourceLocString(nameof(Resources.Progression_RabbidsBigBangStars)), 
                value: score, 
                max: maxScore),
        };

        yield return new SerializableGameProgressionSlot<Unity_PlayerPrefs>(null, 0, score, maxScore, progressItems, context, saveData, saveFile.Name);

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}
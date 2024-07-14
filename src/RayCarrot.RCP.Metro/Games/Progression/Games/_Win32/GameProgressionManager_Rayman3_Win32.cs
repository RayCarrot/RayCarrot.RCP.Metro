using System.IO;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman3_Win32 : GameProgressionManager
{
    public GameProgressionManager_Rayman3_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory + "GAMEDATA" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        IOSearchPattern? dir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "GAMEDATA" + "SaveGame", SearchOption.TopDirectoryOnly, "*.sav"));

        if (dir == null)
            yield break;

        int index = 0;

        using RCPContext context = new(dir.DirPath);

        foreach (FileSystemPath filePath in dir.GetFiles())
        {
            string fileName = filePath.Name;

            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, fileName);

            R3SaveFile? saveData = await context.ReadFileDataAsync<R3SaveFile>(fileName, new R3SaveEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            Logger.Info("Slot has been deserialized");

            IReadOnlyList<GameProgressionDataItem> progressItems = Rayman3Progression.CreateProgressionItems(saveData, out int collectiblesCount, out int maxCollectiblesCount);

            yield return new SerializableGameProgressionSlot<R3SaveFile>($"{filePath.RemoveFileExtension().Name}", index, collectiblesCount, maxCollectiblesCount, progressItems, context, saveData, fileName);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);

            index++;
        }
    }
}
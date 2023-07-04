using System.IO;
using BinarySerializer.Ray1;
using BinarySerializer.Ray1.PC;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman1_MsDos : GameProgressionManager
{
    public GameProgressionManager_Rayman1_MsDos(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
        new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, "*.cfg", "1", 0),
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath? installDir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir, SearchOption.TopDirectoryOnly, "*.SAV"))?.DirPath;

        if (installDir == null)
            yield break;

        using RCPContext context = new(installDir);
        Ray1Settings settings = new(Ray1EngineVersion.PC);
        context.AddSettings(settings);

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            string fileName = $"RAYMAN{saveIndex + 1}.SAV";

            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, saveIndex);

            SaveSlot? saveData = await context.ReadFileDataAsync<SaveSlot>(fileName, new SaveEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

            IReadOnlyList<GameProgressionDataItem> dataItems = Rayman1Progression.CreateProgressionItems(
                saveData, out int collectiblesCount, out int maxCollectiblesCount);

            yield return new SerializableGameProgressionSlot<SaveSlot>(
                name: saveData.SaveName.ToUpper(), 
                index: saveIndex, 
                collectiblesCount: collectiblesCount, 
                totalCollectiblesCount: maxCollectiblesCount, 
                dataItems: dataItems, 
                context: context, 
                serializable: saveData, 
                fileName: fileName);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}
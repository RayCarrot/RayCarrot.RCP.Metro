using System.IO;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman1 : GameProgressionManager
{
    public GameProgressionManager_Rayman1(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.cfg", "1", 0),
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

            PC_SaveFile? saveData = await context.ReadFileDataAsync<PC_SaveFile>(fileName, new PC_SaveEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

            // Get total amount of cages
            int cages = saveData.Wi_Save_Zone.Sum(x => x.Cages);

            GameProgressionDataItem[] dataItems =
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.R1_Cage, 
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: cages, 
                    max: 102),
                new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.R1_Continue, 
                    header: new ResourceLocString(nameof(Resources.Progression_Continues)),
                    value: saveData.ContinuesCount),
                new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.R1_Life, 
                    header: new ResourceLocString(nameof(Resources.Progression_Lives)),
                    value: saveData.StatusBar.LivesCount),
            };

            yield return new SerializableGameProgressionSlot<PC_SaveFile>(
                name: saveData.SaveName.ToUpper(), 
                index: saveIndex, 
                collectiblesCount: cages, 
                totalCollectiblesCount: 102, 
                dataItems: dataItems, 
                context: context, 
                serializable: saveData, 
                fileName: fileName);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}
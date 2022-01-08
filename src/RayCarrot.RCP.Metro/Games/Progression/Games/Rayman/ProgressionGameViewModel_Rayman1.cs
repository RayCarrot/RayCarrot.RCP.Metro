using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer.Ray1;
using NLog;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_Rayman1 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_Rayman1() : base(Games.Rayman1) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
        new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "1", 0),
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath? installDir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir, SearchOption.TopDirectoryOnly, "*.SAV"))?.DirPath;

        if (installDir == null)
            yield break;

        using RCPContext context = new(installDir);

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            string fileName = $"RAYMAN{saveIndex + 1}.SAV";

            Logger.Info("{0} slot {1} is being loaded...", Game, saveIndex);

            PC_SaveFile? saveData = await context.ReadFileDataAsync<PC_SaveFile>(fileName, new PC_SaveEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", Game);
                continue;
            }

            Logger.Info("{0} slot has been deserialized", Game);

            // Get total amount of cages
            int cages = saveData.Wi_Save_Zone.Sum(x => x.Cages);

            ProgressionDataViewModel[] dataItems =
            {
                // TODO-UPDATE: Localize
                new ProgressionDataViewModel(
                    isPrimaryItem: true, 
                    icon: ProgressionIcon.R1_Cage, 
                    header: new ConstLocString("Cages"), 
                    value: cages, 
                    max: 102),
                new ProgressionDataViewModel(
                    isPrimaryItem: false, 
                    icon: ProgressionIcon.R1_Continue, 
                    header: new ConstLocString("Continues"), 
                    value: saveData.ContinuesCount),
                new ProgressionDataViewModel(
                    isPrimaryItem: false, 
                    icon: ProgressionIcon.R1_Life, 
                    header: new ConstLocString("Lives"), 
                    value: saveData.StatusBar.LivesCount),
            };

            yield return new SerializableProgressionSlotViewModel<PC_SaveFile>(
                game: this, 
                name: new ConstLocString(saveData.SaveName.ToUpper()), 
                index: saveIndex, 
                collectiblesCount: cages, 
                totalCollectiblesCount: 102, 
                dataItems: dataItems, 
                context: context, 
                serializable: saveData, 
                fileName: fileName);

            Logger.Info("{0} slot has been loaded", Game);
        }
    }
}
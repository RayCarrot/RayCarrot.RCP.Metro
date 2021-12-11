using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

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
        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            FileSystemPath filePath = InstallDir + $"RAYMAN{saveIndex + 1}.SAV";

            Logger.Info("{0} slot {1} is being loaded...", Game, saveIndex);

            Ray1Settings settings = Ray1Settings.GetDefaultSettings(Ray1Game.Rayman1, Platform.PC);
            Rayman1PCSaveData? saveData = await SerializeFileDataAsync<Rayman1PCSaveData>(fileSystem, filePath, settings, new Rayman12PCSaveDataEncoder());

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
                new ProgressionDataViewModel(true, GameProgression_Icon.R1_Cage, cages, 102),
                new ProgressionDataViewModel(false, GameProgression_Icon.R1_Continue, saveData.ContinuesCount),
                new ProgressionDataViewModel(false, GameProgression_Icon.R1_Life, saveData.StatusBar.LivesCount),
            };

            yield return new ProgressionSlotViewModel(new ConstLocString(saveData.SaveName.ToUpper()), saveIndex, cages, 102, dataItems)
            {
                FilePath = filePath
            };

            Logger.Info("{0} slot has been loaded", Game);
        }
    }
}
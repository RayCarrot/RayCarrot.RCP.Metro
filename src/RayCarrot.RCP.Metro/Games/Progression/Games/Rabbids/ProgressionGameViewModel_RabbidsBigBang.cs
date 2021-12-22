using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RabbidsBigBang : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RabbidsBigBang() : base(Games.RabbidsBigBang) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => GameManager_WinStore.GetWinStoreBackupDirs(Game.GetManager<GameManager_WinStore>().FullPackageName);

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        string packageName = Game.GetManager<GameManager_WinStore>().FullPackageName;

        FileSystemPath saveFile = fileSystem.GetFile(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + 
                                                     "Packages" + packageName + "LocalState" + "playerprefs.dat");

        using RCPContext context = new(saveFile.Parent);

        Logger.Info("{0} save is being loaded...", Game);

        BinarySerializerSettings settings = new(Endian.Little, Encoding.UTF8);
        Unity_PlayerPrefs? saveData = await SerializeFileDataAsync<Unity_PlayerPrefs>(context, saveFile.Name);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", Game);
            yield break;
        }

        Logger.Info("{0} save has been deserialized", Game);

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

        ProgressionDataViewModel[] progressItems =
        {
            // TODO-UPDATE: Localize
            new ProgressionDataViewModel(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RabbidsBigBang_Score, 
                header: new ConstLocString("Stars"), 
                value: score, 
                max: maxScore),
        };

        yield return new BinarySerializableProgressionSlotViewModel<Unity_PlayerPrefs>(this, null, 0, score, maxScore, progressItems, context, saveData, saveFile.Name);

        Logger.Info("{0} save has been loaded", Game);
    }
}
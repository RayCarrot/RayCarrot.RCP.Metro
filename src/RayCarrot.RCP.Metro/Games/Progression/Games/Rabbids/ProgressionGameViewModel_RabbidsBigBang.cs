using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RabbidsBigBang : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RabbidsBigBang() : base(Games.RabbidsBigBang) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => GameManager_WinStore.GetWinStoreBackupDirs(Game.GetManager<GameManager_WinStore>().FullPackageName);

    protected override async Task LoadSlotsAsync()
    {
        string packageName = Game.GetManager<GameManager_WinStore>().FullPackageName;
        FileSystemPath saveFile = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + packageName + "LocalState" + "playerprefs.dat";

        if (!saveFile.FileExists)
            return;

        Logger.Info("Rabbids Big Bang save is being loaded...");

        Unity_PlayerPrefs saveData = await Task.Run(() =>
        {
            // Deserialize the data
            BinarySerializerSettings settings = new(Endian.Little, Encoding.UTF8);
            return BinarySerializableHelpers.ReadFromFile<Unity_PlayerPrefs>(saveFile, settings, Services.App.GetBinarySerializerLogger());
        });

        Logger.Info("Rabbids Big Bang save has been deserialized");

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

        Slots.Add(new ProgressionSlotViewModel(null, 0, score, maxScore, new ProgressionDataViewModel[]
        {
            new ProgressionDataViewModel(true, GameProgression_Icon.RabbidsBigBang_Score, score, maxScore),
        }));

        Logger.Info("Rabbids Big Bang save has been loaded");
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanDesigner : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanDesigner(Games game) : base(game) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override async Task LoadSlotsAsync()
    {
        FileSystemPath saveDir = InstallDir + "PCMAP";

        Logger.Info("Rayman Designer saves from {0} is being loaded...", saveDir.Name);

        // Make sure the directory exists
        if (!saveDir.DirectoryExists)
        {
            Logger.Info("Saves were not loaded due to not being found");
            return;
        }

        string[] shortWorldNames = { "", "JUN", "MUS", "MON", "IMA", "CAV", "CAK" };
        string[] longWorldNames = { "", "Jungle", "Music", "Mountain", "Image", "Cave", "Cake" };

        List<ProgressionDataViewModel> progressItems = new();

        // Find every .sct file
        foreach (var save in Directory.GetFiles(saveDir, "*.sct", SearchOption.TopDirectoryOnly).Select(sct =>
        {
            string fileName = ((FileSystemPath)sct).RemoveFileExtension().Name;

            if (fileName.Length != 5)
                return null;

            string worldStr = fileName.Substring(0, 3);
            string levStr = fileName.Substring(3, 2);

            int world = shortWorldNames.FindItemIndex(x => x == worldStr);
            int lev = Int32.TryParse(levStr, out int parsedLev) ? parsedLev : -1;

            if (world < 1 || lev < 1)
                return null;

            return new
            {
                FilePath = (FileSystemPath)sct,
                World = world,
                Level = lev
            };
        }).Where(x => x != null).OrderBy(x => x!.World).ThenBy(x => x!.Level))
        {
            int value = await Task.Run(() =>
            {
                // Open the file in a stream
                using FileStream fileStream = File.Open(save!.FilePath, FileMode.Open, FileAccess.Read);

                // Deserialize the data
                Ray1Settings settings = Ray1Settings.GetDefaultSettings(Ray1Game.RayKit, Platform.PC);
                RaymanDesignerSaveData saveData = BinarySerializableHelpers.ReadFromStream<RaymanDesignerSaveData>(fileStream, settings, Services.App.GetBinarySerializerLogger(save.FilePath.Name));

                // Get the save value
                return saveData.GetDecodedValue(save.World, save.Level, Game == Games.RaymanDesigner ? RaymanDesignerSaveData.SaveRevision.KIT : RaymanDesignerSaveData.SaveRevision.FAN_60N);
            });

            if (value == -1)
            {
                Logger.Warn("Invalid save value for {0}", save!.FilePath.Name);
                continue;
            }

            // Get the time
            TimeSpan time = TimeSpan.FromMilliseconds(1000d / 60 * value);

            progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.R1_Flag, new ConstLocString($"{longWorldNames[save!.World]} {save.Level}: {time:mm\\:ss\\.fff}")));
        }

        int levelsCount = Game switch
        {
            Games.RaymanDesigner => 24,
            Games.RaymanByHisFans => 40,
            Games.Rayman60Levels => 60,
            _ => -1
        };

        int levelsFinished = progressItems.Count;

        // Add levels completed
        progressItems.Insert(0, new ProgressionDataViewModel(true, GameProgression_Icon.R1_Flag, levelsFinished, levelsCount));

        Slots.Add(new ProgressionSlotViewModel(null, 0, levelsFinished, levelsCount, progressItems));

        Logger.Info("Rayman 3 slot has been loaded");
    }
}
﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_Rayman3 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_Rayman3() : base(Games.Rayman3) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir() + "GAMEDATA" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync()
    {
        FileSystemPath saveDir = InstallDir + "GAMEDATA" + "SaveGame";

        int index = 0;

        foreach (FileSystemPath filePath in Directory.GetFiles(saveDir, "*.sav", SearchOption.TopDirectoryOnly))
        {
            Logger.Info("Rayman 3 slot {0} is being loaded...", filePath.Name);

            Rayman3PCSaveData saveData = await Task.Run(() =>
            {
                // Open the file in a stream
                using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);

                // Create a memory stream
                using MemoryStream memStream = new MemoryStream();

                // Decode the data
                new Rayman3SaveDataEncoder().Decode(fileStream, memStream);

                // Set the position
                memStream.Position = 0;

                // Deserialize the data
                OpenSpaceSettings settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman3, Platform.PC);
                return BinarySerializableHelpers.ReadFromStream<Rayman3PCSaveData>(memStream, settings, Services.App.GetBinarySerializerLogger(filePath.Name));
            });

            Logger.Info("Slot has been deserialized");

            NumberFormatInfo formatInfo = new()
            {
                NumberGroupSeparator = " ",
                NumberDecimalDigits = 0
            };

            // Create the collection with items for each level + general information
            ProgressionDataViewModel[] progressItems = 
            {
                new ProgressionDataViewModel(true, GameProgression_Icon.R3_Cage, saveData.TotalCages, 60),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_TotalHeader}: {saveData.TotalScore.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level1Header}: {saveData.Levels[0].Score.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level2Header}: {saveData.Levels[1].Score.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level3Header}: {saveData.Levels[2].Score.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level4Header}: {saveData.Levels[3].Score.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level5Header}: {saveData.Levels[4].Score.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level6Header}: {saveData.Levels[5].Score.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level7Header}: {saveData.Levels[6].Score.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level8Header}: {saveData.Levels[7].Score.ToString("n", formatInfo)}")),
                new ProgressionDataViewModel(false, GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level9Header}: {saveData.Levels[8].Score.ToString("n", formatInfo)}"))
            };

            yield return new ProgressionSlotViewModel(new ConstLocString($"{filePath.RemoveFileExtension().Name}"), index, saveData.TotalCages, 60, progressItems)
            {
                FilePath = filePath
            };

            Logger.Info("Rayman 3 slot has been loaded");

            index++;
        }
    }
}
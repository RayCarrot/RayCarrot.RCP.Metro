using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_Rayman1 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_Rayman1() : base(Games.Rayman1) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override async Task LoadSlotsAsync()
    {
        if (!InstallDir.DirectoryExists)
            return;

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            FileSystemPath filePath = InstallDir + $"RAYMAN{saveIndex + 1}.SAV";

            if (!filePath.Exists)
                continue;

            Logger.Info("Rayman 1 slot {0} is being loaded...", saveIndex);

            Rayman1PCSaveData saveData = await Task.Run(() =>
            {
                // Open the file in a stream
                using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);

                // Create a memory stream
                using MemoryStream memStream = new();

                // Decode the data
                new Rayman12PCSaveDataEncoder().Decode(fileStream, memStream);

                // Set the position
                memStream.Position = 0;

                // Deserialize and return the data
                return BinarySerializableHelpers.ReadFromStream<Rayman1PCSaveData>(memStream, Ray1Settings.GetDefaultSettings(Ray1Game.Rayman1, Platform.PC), Services.App.GetBinarySerializerLogger(filePath.Name));
            });

            Logger.Info("Rayman 1 slot has been deserialized");

            // Get total amount of cages
            int cages = saveData.Wi_Save_Zone.Sum(x => x.Cages);

            Slots.Add(new ProgressionSlotViewModel(new ConstLocString(saveData.SaveName.ToUpper()), saveIndex, cages, 102, new ProgressionDataViewModel[]
            {
                new ProgressionDataViewModel(true, GameProgression_Icon.R1_Cage, cages, 102),
                new ProgressionDataViewModel(false, GameProgression_Icon.R1_Continue, saveData.ContinuesCount),
                new ProgressionDataViewModel(false, GameProgression_Icon.R1_Life, saveData.StatusBar.LivesCount),
            })
            {
                FilePath = filePath
            });

            Logger.Info("Rayman 1 slot has been loaded");
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanLegends : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanLegends() : base(Games.RaymanLegends) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Gets the level ID's for each level
    /// </summary>
    protected Dictionary<uint, string> GetLevelIDs => new Dictionary<uint, string>
    {
        { 4068998406, "1-1" },
        { 962968486, "1-2" },
        { 2917370473, "1-3" },
        { 2930639107, "1-4" },
        { 1761301024, "1-5" },
        { 3890960063, "1-6" },
        { 1461226270, "2-1" },
        { 60047882, "2-2" },
        { 1069670671, "2-3" },
        { 4107829653, "2-4" },
        { 1997164622, "2-5" },
        { 2720538894, "3-1" },
        { 1753149557, "3-2" },
        { 3767660219, "3-3" },
        { 1403748227, "3-4" },
        { 3610402831, "3-5" },
        { 3244269653, "4-1" },
        { 2523282745, "4-2" },
        { 193580080, "4-3" },
        { 80857532, "4-4" },
        { 532378801, "4-5" },
        { 304308657, "4-6" },
        { 3703754575, "5-1" },
        { 576210007, "5-2" },
        { 897150152, "5-3" },
        { 2207233233, "5-4" },
        { 941235443, "5-5" },
        { 3600674311, "5-6" }
    };

    protected override async Task LoadSlotsAsync()
    {
        FileSystemPath saveDir = Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends";

        foreach (FileSystemPath saveFile in Directory.GetDirectories(saveDir).Select(x => new FileSystemPath(x) + "RaymanSave_0"))
        {
            Logger.Info("Rayman Legends slot {0} is being loaded...", saveFile.Parent.Name);

            // Make sure the file exists
            if (!saveFile.FileExists)
            {
                Logger.Info("Slot was not loaded due to not being found");
                continue;
            }

            // Deserialize and return the data
            UbiArtSettings settings = UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanLegends, Platform.PC);
            LegendsPCSaveData.PersistentGameData_Universe saveData = await Task.Run(() => BinarySerializableHelpers.ReadFromFile<LegendsPCSaveData>(saveFile, settings, Services.App.GetBinarySerializerLogger(saveFile.Name)).SaveData);

            Logger.Info("Slot has been deserialized");

            // Create the collection with items for each time trial level + general information
            List<ProgressionDataViewModel> progressItems = new();

            // Get the total amount of freed teensies
            int teensies = saveData.Levels.Select(x => x.Value.Object.FreedPrisoners.Length).Sum() + saveData.LuckyTicketRewardList.Count(x => x.Type == 5);

            // Add general progress info
            progressItems.Add(new ProgressionDataViewModel(true, GameProgression_Icon.RL_Teensy, teensies, 700));
            progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RL_Lum, saveData.Score.LocalLumsCount));

            // Add rank
            progressItems.Add(new ProgressionDataViewModel(true, Enum.Parse(typeof(GameProgression_Icon), $"RL_Rank{saveData.Profile.StatusIcon}").CastTo<GameProgression_Icon>(), (int)saveData.Profile.StatusIcon, 11));

            // Add cups
            progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RL_Bronze, (int)saveData.Profile.BronzeMedals));
            progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RL_Silver, (int)saveData.Profile.SilverMedals));
            progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RL_Gold, (int)saveData.Profile.GoldMedals));
            progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RL_Diamond, (int)saveData.Profile.DiamondMedals));

            // Get the level IDs
            Dictionary<uint, string> lvlIds = GetLevelIDs;

            // Add invasion times
            progressItems.AddRange(saveData.Levels.
                Select(x => x.Value.Object).
                Where(x => x.BestTime > 0).
                Select(x => (lvlIds[x.Id.ID], x.BestTime)).
                Select(x => new ProgressionDataViewModel(false,
                    Enum.Parse(typeof(GameProgression_Icon), $"RL_Inv_{x.Item1.Replace("-", "_")}").CastTo<GameProgression_Icon>(),
                    new ConstLocString($"{x.Item1}: {TimeSpan.FromMilliseconds(x.BestTime * 1000):mm\\:ss\\.fff}"),
                    new ResourceLocString($"RL_LevelName_{x.Item1.Replace("-", "_")}"))).
                OrderBy(x => x.Text.Value));

            Slots.Add(new ProgressionSlotViewModel(new ConstLocString(saveData.Profile.Name), 0, teensies, 700, progressItems));

            Logger.Info("Rayman Legends slot has been loaded");
        }
    }
}
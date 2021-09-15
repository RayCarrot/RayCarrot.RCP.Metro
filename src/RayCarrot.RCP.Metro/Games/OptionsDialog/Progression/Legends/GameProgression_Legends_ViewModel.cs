using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RayCarrot.Binary;
using NLog;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Legends progression
    /// </summary>
    public class GameProgression_Legends_ViewModel : GameProgression_BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameProgression_Legends_ViewModel() : base(Games.RaymanJungleRun)
        {
            // Get the save data directory
            SaveDir = Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends";
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the level ID's for each level
        /// </summary>
        protected Dictionary<uint, string> GetLevelIDs => new Dictionary<uint, string>()
        {
            {
                4068998406,
                "1-1"
            },
            {
                962968486,
                "1-2"
            },
            {
                2917370473,
                "1-3"
            },
            {
                2930639107,
                "1-4"
            },
            {
                1761301024,
                "1-5"
            },
            {
                3890960063,
                "1-6"
            },
            {
                1461226270,
                "2-1"
            },
            {
                60047882,
                "2-2"
            },
            {
                1069670671,
                "2-3"
            },
            {
                4107829653,
                "2-4"
            },
            {
                1997164622,
                "2-5"
            },
            {
                2720538894,
                "3-1"
            },
            {
                1753149557,
                "3-2"
            },
            {
                3767660219,
                "3-3"
            },
            {
                1403748227,
                "3-4"
            },
            {
                3610402831,
                "3-5"
            },
            {
                3244269653,
                "4-1"
            },
            {
                2523282745,
                "4-2"
            },
            {
                193580080,
                "4-3"
            },
            {
                80857532,
                "4-4"
            },
            {
                532378801,
                "4-5"
            },
            {
                304308657,
                "4-6"
            },
            {
                3703754575,
                "5-1"
            },
            {
                576210007,
                "5-2"
            },
            {
                897150152,
                "5-3"
            },
            {
                2207233233,
                "5-4"
            },
            {
                941235443,
                "5-5"
            },
            {
                3600674311,
                "5-6"
            },
        };

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the progression slot view model for the save data from the specified file
        /// </summary>
        /// <param name="filePath">The slot file path</param>
        /// <returns>The progression slot view model</returns>
        protected GameProgression_BaseSlotViewModel GetProgressionSlotViewModel(FileSystemPath filePath)
        {
            Logger.Info("Legends slot {0} is being loaded...", filePath.Parent.Name);

            // Make sure the file exists
            if (!filePath.FileExists)
            {
                Logger.Info("Slot was not loaded due to not being found");

                return null;
            }

            // Deserialize and return the data
            var saveData = BinarySerializableHelpers.ReadFromFile<LegendsPCSaveData>(filePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanLegends, Platform.PC), Services.App.GetBinarySerializerLogger(filePath.Name)).SaveData;

            Logger.Info("Slot has been deserialized");

            // Create the collection with items for each time trial level + general information
            var progressItems = new List<GameProgression_InfoItemViewModel>();

            // Create the number format info to use
            var formatInfo = new NumberFormatInfo()
            {
                NumberGroupSeparator = " ",
                NumberDecimalDigits = 0
            };

            // Get the total amount of freed teensies
            var teensies = saveData.Levels.Select(x => x.Value.Object.FreedPrisoners.Length).Sum() + saveData.LuckyTicketRewardList.Count(x => x.Type == 5);

            // Set general progress info
            progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.RL_Teensy, new ConstLocString($"{teensies}/700")));
            progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.RL_Lum, new ConstLocString($"{saveData.Score.LocalLumsCount.ToString("n", formatInfo)}")));

            // Set rank
            progressItems.Add(new GameProgression_InfoItemViewModel(Enum.Parse(typeof(GameProgression_Icon), $"RL_Rank{saveData.Profile.StatusIcon}").CastTo<GameProgression_Icon>(), new ConstLocString($"{saveData.Profile.StatusIcon}")));

            // Set cups
            progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.RL_Bronze, new ConstLocString($"{saveData.Profile.BronzeMedals.ToString("n", formatInfo)}")));
            progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.RL_Silver, new ConstLocString($"{saveData.Profile.SilverMedals.ToString("n", formatInfo)}")));
            progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.RL_Gold, new ConstLocString($"{saveData.Profile.GoldMedals.ToString("n", formatInfo)}")));
            progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.RL_Diamond, new ConstLocString($"{saveData.Profile.DiamondMedals.ToString("n", formatInfo)}")));

            Logger.Info("General progress info has been set");

            // Get the level IDs
            var lvlIds = GetLevelIDs;
            
            // Set invasion times
            progressItems.AddRange(saveData.Levels.
                Select(x => x.Value.Object).
                Where(x => x.BestTime > 0).
                Select(x => (lvlIds[x.Id.ID], x.BestTime)).
                Select(x => new GameProgression_InfoItemViewModel(
                    Enum.Parse(typeof(GameProgression_Icon), $"RL_Inv_{x.Item1.Replace("-", "_")}").CastTo<GameProgression_Icon>(), 
                    new ConstLocString($"{x.Item1}: {TimeSpan.FromMilliseconds(x.BestTime * 1000):mm\\:ss\\.fff}"), 
                    new ResourceLocString($"RL_LevelName_{x.Item1.Replace("-", "_")}"))).
                OrderBy(x => x.Content.Value));

            Logger.Info("Invasion progress info has been set");

            // Calculate the percentage
            var percentage = ((teensies / 700d * 100)).ToString("0.##");

            Logger.Info("Slot percentage is {0}%", percentage);

            // Return the data with the collection
            return new GameProgression_Legends_SlotViewModel(new ConstLocString($"{saveData.Profile.Name} ({percentage}%)"), progressItems.ToArray(), filePath, this);
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        protected override void LoadData()
        {
            // Read and set slot data
            ProgressionSlots.AddRange(Directory.GetDirectories(SaveDir).Select(x => new FileSystemPath(x) + "RaymanSave_0").Select(GetProgressionSlotViewModel));
        }

        #endregion
    }
}
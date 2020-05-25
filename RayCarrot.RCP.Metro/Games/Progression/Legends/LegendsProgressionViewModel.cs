using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RayCarrot.Binary;
using RayCarrot.Logging;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Legends progression
    /// </summary>
    public class LegendsProgressionViewModel : BaseProgressionViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LegendsProgressionViewModel() : base(Games.RaymanJungleRun)
        {
            // Get the save data directory
            SaveDir = Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends";
        }

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
        protected ProgressionSlotViewModel GetProgressionSlotViewModel(FileSystemPath filePath)
        {
            RL.Logger?.LogInformationSource($"Legends slot {filePath.Parent.Name} is being loaded...");

            // Make sure the file exists
            if (!filePath.FileExists)
            {
                RL.Logger?.LogInformationSource($"Slot was not loaded due to not being found");

                return null;
            }

            // Deserialize and return the data
            var saveData = BinarySerializableHelpers.ReadFromFile<LegendsPCSaveData>(filePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanLegends, Platform.PC), RCPServices.App.GetBinarySerializerLogger()).SaveData;

            RL.Logger?.LogInformationSource($"Slot has been deserialized");

            // Create the collection with items for each time trial level + general information
            var progressItems = new List<ProgressionInfoItemViewModel>();

            // Create the number format info to use
            var formatInfo = new NumberFormatInfo()
            {
                NumberGroupSeparator = " ",
                NumberDecimalDigits = 0
            };

            // Get the total amount of freed teensies
            var teensies = saveData.Levels.Select(x => x.Value.Object.FreedPrisoners.Length).Sum() + saveData.LuckyTicketRewardList.Count(x => x.Type == 5);

            // Set general progress info
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RL_Teensy, new LocalizedString(() => $"{teensies}/700")));
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RL_Lum, new LocalizedString(() => $"{saveData.Score.LocalLumsCount.ToString("n", formatInfo)}")));

            // Set rank
            progressItems.Add(new ProgressionInfoItemViewModel(Enum.Parse(typeof(ProgressionIcons), $"RL_Rank{saveData.Profile.StatusIcon}").CastTo<ProgressionIcons>(), new LocalizedString(() => $"{saveData.Profile.StatusIcon}")));

            // Set cups
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RL_Bronze, new LocalizedString(() => $"{saveData.Profile.BronzeMedals.ToString("n", formatInfo)}")));
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RL_Silver, new LocalizedString(() => $"{saveData.Profile.SilverMedals.ToString("n", formatInfo)}")));
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RL_Gold, new LocalizedString(() => $"{saveData.Profile.GoldMedals.ToString("n", formatInfo)}")));
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RL_Diamond, new LocalizedString(() => $"{saveData.Profile.DiamondMedals.ToString("n", formatInfo)}")));

            RL.Logger?.LogInformationSource($"General progress info has been set");

            // Get the level IDs
            var lvlIds = GetLevelIDs;
            
            // Set invasion times
            progressItems.AddRange(saveData.Levels.
                Select(x => x.Value.Object).
                Where(x => x.BestTime > 0).
                Select(x => (lvlIds[x.Id.ID], x.BestTime)).
                Select(x => new ProgressionInfoItemViewModel(
                    Enum.Parse(typeof(ProgressionIcons), $"RL_Inv_{x.Item1.Replace("-", "_")}").CastTo<ProgressionIcons>(), 
                    new LocalizedString(() => $"{x.Item1}: {x.BestTime:0.000}"), 
                    new LocalizedString(() => Resources.ResourceManager.GetString($"RL_LevelName_{x.Item1.Replace("-", "_")}")))).
                OrderBy(x => x.Content.Value));

            RL.Logger?.LogInformationSource($"Invasion progress info has been set");

            // Calculate the percentage
            var percentage = ((teensies / 700d * 100)).ToString("0.##");

            RL.Logger?.LogInformationSource($"Slot percentage is {percentage}%");

            // Return the data with the collection
            return new LegendsProgressionSlotViewModel(new LocalizedString(() => $"{saveData.Profile.Name} ({percentage}%)"), progressItems.ToArray(), filePath, this);
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
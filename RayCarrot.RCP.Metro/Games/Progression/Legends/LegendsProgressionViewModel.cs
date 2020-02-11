using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        // TODO: Localize level names
        /// <summary>
        /// Gets the level ID's for each level
        /// </summary>
        public Dictionary<uint, LegendsInvasion> GetLevelIDs => new Dictionary<uint, LegendsInvasion>()
        {
            {
                4068998406,
                new LegendsInvasion("1-1", new LocalizedString(() => "Once upon a Time"))
            },
            {
                962968486,
                new LegendsInvasion("1-2", new LocalizedString(() => "Creepy Castle"))
            },
            {
                2917370473,
                new LegendsInvasion("1-3", new LocalizedString(() => "Enchanted Forest"))
            },
            {
                2930639107,
                new LegendsInvasion("1-4", new LocalizedString(() => "Ropes Course"))
            },
            {
                1761301024,
                new LegendsInvasion("1-5", new LocalizedString(() => "Quick Sand"))
            },
            {
                3890960063,
                new LegendsInvasion("1-6", new LocalizedString(() => "How to Shoot your Dragon"))
            },
            {
                1461226270,
                new LegendsInvasion("2-1", new LocalizedString(() => "Ray and the Beanstalk"))
            },
            {
                60047882,
                new LegendsInvasion("2-2", new LocalizedString(() => "The Winds of Strange"))
            },
            {
                1069670671,
                new LegendsInvasion("2-3", new LocalizedString(() => "Castle in the Clouds"))
            },
            {
                4107829653,
                new LegendsInvasion("2-4", new LocalizedString(() => "Altitude Quickness"))
            },
            {
                1997164622,
                new LegendsInvasion("2-5", new LocalizedString(() => "When Toads Fly"))
            },
            {
                2720538894,
                new LegendsInvasion("3-1", new LocalizedString(() => "What the Duck?"))
            },
            {
                1753149557,
                new LegendsInvasion("3-2", new LocalizedString(() => "Spoiled Rotten"))
            },
            {
                3767660219,
                new LegendsInvasion("3-3", new LocalizedString(() => "I've Got a Filling"))
            },
            {
                1403748227,
                new LegendsInvasion("3-4", new LocalizedString(() => "Snakes on a Cake"))
            },
            {
                3610402831,
                new LegendsInvasion("3-5", new LocalizedString(() => "Lucha Libre Get Away"))
            },
            {
                3244269653,
                new LegendsInvasion("4-1", new LocalizedString(() => "The Mysterious Inflatable Island"))
            },
            {
                2523282745,
                new LegendsInvasion("4-2", new LocalizedString(() => "The Deadly Lights"))
            },
            {
                193580080,
                new LegendsInvasion("4-3", new LocalizedString(() => "Mansion of the Deep"))
            },
            {
                80857532,
                new LegendsInvasion("4-4", new LocalizedString(() => "Infiltration Station"))
            },
            {
                532378801,
                new LegendsInvasion("4-5", new LocalizedString(() => "Elevator Ambush"))
            },
            {
                304308657,
                new LegendsInvasion("4-6", new LocalizedString(() => "There's Always a Bigger Fish"))
            },
            {
                3703754575,
                new LegendsInvasion("5-1", new LocalizedString(() => "Shields Up... and Down"))
            },
            {
                576210007,
                new LegendsInvasion("5-2", new LocalizedString(() => "The Dark Creatures Rise"))
            },
            {
                897150152,
                new LegendsInvasion("5-3", new LocalizedString(() => "The Amazing Maze"))
            },
            {
                2207233233,
                new LegendsInvasion("5-4", new LocalizedString(() => "The Great Lava Pursuit"))
            },
            {
                941235443,
                new LegendsInvasion("5-5", new LocalizedString(() => "Swarmed and Dangerous"))
            },
            {
                3600674311,
                new LegendsInvasion("5-6", new LocalizedString(() => "Hell Breaks Loose"))
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
            RCFCore.Logger?.LogInformationSource($"Legends slot {filePath.Parent.Name} is being loaded...");

            // Make sure the file exists
            if (!filePath.FileExists)
            {
                RCFCore.Logger?.LogInformationSource($"Slot was not loaded due to not being found");

                return null;
            }

            // Deserialize and return the data
            var saveData = LegendsPCSaveData.GetSerializer().Deserialize(filePath).SaveData;

            RCFCore.Logger?.LogInformationSource($"Slot has been deserialized");

            // Create the collection with items for each time trial level + general information
            var progressItems = new List<ProgressionInfoItemViewModel>();

            // Create the number format info to use
            var formatInfo = new NumberFormatInfo()
            {
                NumberGroupSeparator = " ",
                NumberDecimalDigits = 0
            };

            // Get the total amount of freed teensies
            var teensies = saveData.Levels.Select(x => x.Value.Object.FreedPrisoners.Count).Sum() + saveData.LuckyTicketRewardList.Count(x => x.Type == 5);

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

            RCFCore.Logger?.LogInformationSource($"General progress info has been set");

            // Get the level IDs
            var lvlIds = GetLevelIDs;
            
            // Set invasion times
            progressItems.AddRange(saveData.Levels.
                Select(x => x.Value.Object).
                Where(x => x.BestTime > 0).
                Select(x => (lvlIds[x.Id.ID], x.BestTime)).
                Select(x => new ProgressionInfoItemViewModel(
                    Enum.Parse(typeof(ProgressionIcons), $"RL_Inv_{x.Item1.ShortName.Replace("-", "_")}").CastTo<ProgressionIcons>(), 
                    new LocalizedString(() => $"{x.Item1.ShortName}: {x.BestTime:0.000}"), 
                    x.Item1.LongName)).
                OrderBy(x => x.Content.Value));

            RCFCore.Logger?.LogInformationSource($"Invasion progress info has been set");

            // Calculate the percentage
            var percentage = ((teensies / 700d * 100)).ToString("0.##");

            RCFCore.Logger?.LogInformationSource($"Slot percentage is {percentage}%");

            // Return the data with the collection
            return new LegendsProgressionSlotViewModel(new LocalizedString(() => $"{saveData.Profile.Name} ({percentage}%)"), progressItems.ToArray(), filePath, this);
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        /// <returns>The task</returns>
        public override async Task LoadDataAsync()
        {
            RCFCore.Logger?.LogInformationSource($"Progression data for Legends is being loaded...");

            // Run on a new thread
            await Task.Run(() =>
            {
                try
                {
                    // Dispose existing slot view models
                    ProgressionSlots.DisposeAll();

                    RCFCore.Logger?.LogDebugSource($"Existing slots have been disposed");
                    
                    // Clear the collection
                    ProgressionSlots.Clear();

                    // Read and set slot data
                    ProgressionSlots.AddRange(Directory.GetDirectories(SaveDir).Select(x => new FileSystemPath(x) + "RaymanSave_0").Select(GetProgressionSlotViewModel));

                    RCFCore.Logger?.LogInformationSource($"Slots have been loaded");

                    // Remove empty slots
                    ProgressionSlots.RemoveWhere(x => x == null);

                    RCFCore.Logger?.LogDebugSource($"Empty slots have been removed");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Reading Legends save data");
                    throw;
                }
            });
        }

        #endregion

        #region Classes

        /// <summary>
        /// A Rayman Legends invasion
        /// </summary>
        public class LegendsInvasion
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="shortName">The short level name</param>
            /// <param name="longName">The long, localized, level name</param>
            public LegendsInvasion(string shortName, LocalizedString longName)
            {
                ShortName = shortName;
                LongName = longName;
            }

            /// <summary>
            /// The short level name
            /// </summary>
            public string ShortName { get; }

            /// <summary>
            /// The long, localized, level name
            /// </summary>
            public LocalizedString LongName { get; }
        }

        #endregion
    }
}
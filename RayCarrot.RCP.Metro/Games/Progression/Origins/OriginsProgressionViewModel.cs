using Newtonsoft.Json;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Collections.Generic;
using System.Linq;
using RayCarrot.Binary;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Origins progression
    /// </summary>
    public class OriginsProgressionViewModel : BaseProgressionViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public OriginsProgressionViewModel() : base(Games.RaymanJungleRun)
        {
            // Get the save data directory
            SaveDir = Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins" + "Savegame";
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the progression slot view model for the save data from the specified file
        /// </summary>
        /// <param name="filePath">The slot file path</param>
        /// <param name="slotNamegenerator">The generator for the name of the save slot</param>
        /// <returns>The progression slot view model</returns>
        protected ProgressionSlotViewModel GetProgressionSlotViewModel(FileSystemPath filePath, Func<string> slotNamegenerator)
        {
            RCFCore.Logger?.LogInformationSource($"Origins slot {filePath.Name} is being loaded...");

            // Make sure the file exists
            if (!filePath.FileExists)
            {
                RCFCore.Logger?.LogInformationSource($"Slot was not loaded due to not being found");

                return null;
            }

            // Deserialize and get the data
            var saveData = BinarySerializableHelpers.ReadFromFile<OriginsPCSaveData>(filePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanOrigins, Platform.PC), RCFRCP.App.GetBinarySerializerLogger()).SaveData;

            RCFCore.Logger?.LogInformationSource($"Slot has been deserialized");

            // Get the level configuration
            var lvlConfig = JsonConvert.DeserializeObject<ROLevelConfig>(Files.RO_LevelConfig);

            int completed = 0;
            int cageMaps = 0;
            int lumAttack1 = 0;
            int lumAttack2 = 0;
            int lumAttack3 = 0;
            int timeAttack1 = 0;
            int timeAttack2 = 0;

            // Get number of levels where each mission has been completed
            foreach (var lvl in saveData.Levels)
            {
                // Get the configuration for the level
                var level = lvlConfig.GetLevel(lvl.Key.ID);
                var mission = lvlConfig.GetMission(lvl.Key.ID);

                // Make sure it's a normal level, i.e. it has a medallion
                if (level == null || mission == null)
                    continue;

                // Check if the level has been completed
                if (lvl.Value.Object.LevelState == OriginsPCSaveData.SPOT_STATE.COMPLETED)
                    completed++;

                // Get the number of completed cage maps (between 0-2)
                cageMaps += lvl.Value.Object.CageMapPassedDoors.Length;

                // Get the best time attack score
                var timeAttack = lvl.Value.Object.BestTimeAttack;

                // Compare the time attack score with the targets
                if (timeAttack <= level.Time1)
                    timeAttack1++;
                if (timeAttack <= level.Time2)
                    timeAttack2++;

                // Get the best lum attack score
                var lumAttack = lvl.Value.Object.BestLumAttack;

                // Compare the lum attack score with the targets
                if (lumAttack >= mission.LumAttack1)
                    lumAttack1++;
                if (lumAttack >= mission.LumAttack2)
                    lumAttack2++;
                if (lumAttack >= mission.LumAttack3)
                    lumAttack3++;
            }

            // Create the collection with items for each time trial level + general information
            var progressItems = new List<ProgressionInfoItemViewModel>();

            // Get the number of Electoons
            var electoons = 
                // Cages
                cageMaps +
                // Levels completed
                completed + 
                // Lum attack 1
                lumAttack1 + 
                // Lum attack 2
                lumAttack2 + 
                // Time attack 1
                timeAttack1;
            var teeth = saveData.Levels.Select(x => x.Value.Object.ISDs.Select(y => y.Value.Object.TakenTooth.Length)).SelectMany(x => x).Sum();

            // Set general progress info
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RO_Electoon, new LocalizedString(() => $"{electoons}/246")));
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RO_RedTooth, new LocalizedString(() => $"{teeth}/10")));
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RO_Medal, new LocalizedString(() => $"{lumAttack3}/51")));
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RO_Trophy, new LocalizedString(() => $"{timeAttack2}/31")));

            RCFCore.Logger?.LogInformationSource($"General progress info has been set");

            // Calculate the percentage
            var percentage = ((electoons / 246d * 25) + (teeth / 10d * 25) + (lumAttack3 / 51d * 25) + (timeAttack2 / 31d * 25)).ToString("0.##");

            RCFCore.Logger?.LogInformationSource($"Slot percentage is {percentage}%");

            // Return the data with the collection
            return new OriginsProgressionSlotViewModel(new LocalizedString(() => $"{slotNamegenerator()} ({percentage}%)"), progressItems.ToArray(), filePath, this);
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        protected override void LoadData()
        {
            // Read and set slot data
            ProgressionSlots.Add(GetProgressionSlotViewModel(SaveDir + "Savegame_0", () => String.Format(Resources.Progression_GenericSlot, "1")));
            ProgressionSlots.Add(GetProgressionSlotViewModel(SaveDir + "Savegame_1", () => String.Format(Resources.Progression_GenericSlot, "2")));
            ProgressionSlots.Add(GetProgressionSlotViewModel(SaveDir + "Savegame_2", () => String.Format(Resources.Progression_GenericSlot, "3")));
        }

        #endregion

        #region Classes

        /// <summary>
        /// Data for Rayman Origins level configuration
        /// </summary>
        protected class ROLevelConfig
        {
            #region Constructor

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="levels">The levels</param>
            /// <param name="missions">The mission types</param>
            public ROLevelConfig(Level[] levels, Mission[] missions)
            {
                Levels = levels;
                Missions = missions;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// The levels
            /// </summary>
            public Level[] Levels { get; }

            /// <summary>
            /// The mission types
            /// </summary>
            public Mission[] Missions { get; }

            #endregion

            #region Public Methods

            /// <summary>
            /// Gets the level which matches the tag
            /// </summary>
            /// <param name="tag">The level tag</param>
            /// <returns>The level, or null if not found</returns>
            public Level GetLevel(uint tag) => Levels.FindItem(x => x.Tag == tag);

            /// <summary>
            /// Gets the mission which matches the level tag
            /// </summary>
            /// <param name="tag">The level tag</param>
            /// <returns>The mission, or null if not found</returns>
            public Mission GetMission(uint tag) => Missions.FindItem(x => x.Type == GetLevel(tag)?.Type);

            #endregion

            public class Level
            {
                public Level(int time1, int time2, uint tag, uint type)
                {
                    Time1 = time1;
                    Time2 = time2;
                    Tag = tag;
                    Type = type;
                }

                public int Time1 { get; }

                public int Time2 { get; }
                
                public uint Tag { get; }
                
                public uint Type { get; }
            }

            public class Mission
            {
                public Mission(uint type, int lumAttack1, int lumAttack2, int lumAttack3)
                {
                    Type = type;
                    LumAttack1 = lumAttack1;
                    LumAttack2 = lumAttack2;
                    LumAttack3 = lumAttack3;
                }

                public uint Type { get; }

                public int LumAttack1 { get; }
                
                public int LumAttack2 { get; }
                
                public int LumAttack3 { get; }
            }
        }

        #endregion
    }
}
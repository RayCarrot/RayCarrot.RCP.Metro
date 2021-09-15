using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using RayCarrot.UI;
using System;
using RayCarrot.Binary;
using NLog;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Jungle Run progression
    /// </summary>
    public class GameProgression_JungleRun_ViewModel : GameProgression_BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameProgression_JungleRun_ViewModel() : base(Games.RaymanJungleRun)
        {
            // Get the save data directory
            SaveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + Game.GetManager<GameManager_WinStore>().FullPackageName + "LocalState";
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the progression slot view model for the save data from the specified file
        /// </summary>
        /// <param name="fileName">The slot file name, relative to the save directory</param>
        /// <param name="slotNamegenerator">The generator for the name of the save slot</param>
        /// <returns>The progression slot view model</returns>
        protected GameProgression_BaseSlotViewModel GetProgressionSlotViewModel(FileSystemPath fileName, Func<string> slotNamegenerator)
        {
            Logger.Info("Jungle Run slot {0} is being loaded...", fileName.Name);

            // Get the file path
            var filePath = SaveDir + fileName;

            // Make sure the file exists
            if (!filePath.FileExists)
            {
                Logger.Info("Slot was not loaded due to not being found");

                return null;
            }

            // Deserialize and return the data
            var saveData = BinarySerializableHelpers.ReadFromFile<JungleRunPCSaveData>(filePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanJungleRun, Platform.PC), Services.App.GetBinarySerializerLogger(filePath.Name));

            Logger.Info("Slot has been deserialized");

            // Create the collection with items for each time trial level + general information
            var progressItems = new GameProgression_InfoItemViewModel[(saveData.Levels.Length / 10) + 2];

            // Get data values
            int collectedLums = 0;
            int availableLums = 0;
            int collectedTeeth = 0;
            int availableTeeth = saveData.Levels.Length;

            Logger.Trace("Levels are being enumerated...");

            // Enumerate each level
            for (int i = 0; i < saveData.Levels.Length; i++)
            {
                // Get the level data
                var levelData = saveData.Levels[i];

                // Check if the level is a normal level
                if ((i + 1) % 10 != 0)
                {
                    Logger.Trace("Level index {0} is a normal level", i);

                    // Get the collected lums
                    collectedLums += levelData.LumsRecord;
                    availableLums += 100;

                    Logger.Trace("{0} Lums have been collected", levelData.LumsRecord);

                    // Check if the level is 100% complete
                    if (levelData.LumsRecord >= 100)
                        collectedTeeth++;

                    continue;
                }

                Logger.Trace("Level index {0} is a time trial level", i);

                // Make sure the level has been completed
                if (levelData.RecordTime == 0)
                {
                    Logger.Trace("Level has not been completed");

                    continue;
                }

                Logger.Trace("Level has been completed with the record time {0}", levelData.RecordTime);

                collectedTeeth++;

                // Get the level number, starting at 10
                var fullLevelNumber = (i + 11).ToString();

                // Get the world and level numbers
                var worldNum = fullLevelNumber[0].ToString();
                var lvlNum = fullLevelNumber[1].ToString();

                // If the level is 0, correct the numbers to be level 10
                if (lvlNum == "0")
                {
                    worldNum = (Int32.Parse(worldNum) - 1).ToString();
                    lvlNum = "10";
                }
                
                // Create the view model
                progressItems[((i + 1) / 10) - 1 + 2] = new GameProgression_InfoItemViewModel(GameProgression_Icon.RO_Clock, new ConstLocString($"{worldNum}-{lvlNum}: {new TimeSpan(0, 0, 0, 0, (int)levelData.RecordTime):mm\\:ss\\.fff}"));
            }

            // Set general progress info
            progressItems[0] = new GameProgression_InfoItemViewModel(GameProgression_Icon.RO_Lum, new ConstLocString($"{collectedLums}/{availableLums}"));
            progressItems[1] = new GameProgression_InfoItemViewModel(GameProgression_Icon.RO_RedTooth, new ConstLocString($"{collectedTeeth}/{availableTeeth}"));

            Logger.Info("General progress info has been set");

            // Calculate the percentage
            var percentage = ((collectedLums / (double)availableLums * 50) + (collectedTeeth / (double)availableTeeth * 50)).ToString("0.##");

            Logger.Info("Slot percentage is {0}%", percentage);

            // Return the data with the collection
            return new GameProgression_JungleRun_SlotViewModel(new GeneratedLocString(() => $"{slotNamegenerator()} ({percentage}%)"), progressItems, filePath, this);
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        protected override void LoadData()
        {
            // Read and set slot data
            ProgressionSlots.AddRange(new GameProgression_BaseSlotViewModel[]
            {
                GetProgressionSlotViewModel("slot1.dat", () => String.Format(Resources.Progression_GenericSlot, "1")),
                GetProgressionSlotViewModel("slot2.dat", () => String.Format(Resources.Progression_GenericSlot, "2")),
                GetProgressionSlotViewModel("slot3.dat", () => String.Format(Resources.Progression_GenericSlot, "3"))
            });
        }

        #endregion
    }
}
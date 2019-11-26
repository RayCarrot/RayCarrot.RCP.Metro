using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Jungle Run progression
    /// </summary>
    public class JungleRunProgressionViewModel : BaseProgressionViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public JungleRunProgressionViewModel() : base(Games.RaymanJungleRun)
        {
            // Get the save data directory
            SaveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + Game.GetManager<RCPWinStoreGame>().FullPackageName + "LocalState";
        }

        #endregion

        #region Public Override Properties

        /// <summary>
        /// Indicates if the progression data is available
        /// </summary>
        public override bool IsAvailable => Directory.EnumerateFiles(SaveDir).Any(x => new FileSystemPath(x).FileExtension == ".dat");

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the progression slot view model for the save data from the specified file
        /// </summary>
        /// <param name="fileName">The slot file name, relative to the save directory</param>
        /// <param name="slotNamegenerator">The generator for the name of the save slot</param>
        /// <returns>The progression slot view model</returns>
        protected ProgressionSlotViewModel GetProgressionSlotViewModel(FileSystemPath fileName, Func<string> slotNamegenerator)
        {
            // Get the file path
            var filePath = SaveDir + fileName;

            // Make sure the file exists
            if (!filePath.FileExists)
                return null;

            // Deserialize and return the data
            var saveData = new JungleRunSaveDataSerializer().Deserialize(filePath);

            // Create the collection with items for each time trial level + general information
            var progressItems = new ProgressionInfoItemViewModel[saveData.Levels.Count + 2];

            // Get data values
            int collectedLums = 0;
            int availableLums = 0;
            int collectedTeeth = 0;
            int availableTeeth = saveData.Levels.Count;

            // Enumerate each level
            for (int i = 0; i < saveData.Levels.Count; i++)
            {
                // Get the level data
                var levelData = saveData.Levels[i];

                // Check if the level is a normal level
                if ((i + 1) % 10 != 0)
                {
                    // Get the collected lums
                    collectedLums += levelData.LumsRecord;
                    availableLums += 100;

                    // Check if the level is 100% complete
                    if (levelData.LumsRecord >= 100)
                        collectedTeeth++;

                    continue;
                }

                // Make sure the level has been completed
                if (levelData.RecordTime == TimeSpan.Zero)
                    continue;

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
                progressItems[i + 2] = new ProgressionInfoItemViewModel(ProgressionIcons.Clock, $"{worldNum}-{lvlNum}: {levelData.RecordTime:mm\\:ss\\:fff}");
            }

            // Set general progress info
            progressItems[0] = new ProgressionInfoItemViewModel(ProgressionIcons.Lum, $"{collectedLums}/{availableLums}");
            progressItems[1] = new ProgressionInfoItemViewModel(ProgressionIcons.RedTooth, $"{collectedTeeth}/{availableTeeth}");

            // Calculate the percentage
            var percentage = ((collectedLums / (double)availableLums * 50) + (collectedTeeth / (double)availableTeeth * 50)).ToString("0.##");

            // Return the data with the collection
            return new JungleRunProgressionSlotViewModel(() => $"{slotNamegenerator()} ({percentage}%)", progressItems, filePath, this);
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        /// <returns>The task</returns>
        public override async Task LoadDataAsync()
        {
            // Run on a new thread
            await Task.Run(() =>
            {
                try
                {
                    // Dispose existing slot view models
                    ProgressionSlots?.ForEach(x => x.Dispose());

                    // Read and set slot data
                    ProgressionSlots = new ObservableCollection<ProgressionSlotViewModel>()
                    {
                        GetProgressionSlotViewModel("slot1.dat", () => String.Format(Resources.Progression_GenericSlot, "1")),
                        GetProgressionSlotViewModel("slot2.dat", () => String.Format(Resources.Progression_GenericSlot, "2")),
                        GetProgressionSlotViewModel("slot3.dat", () => String.Format(Resources.Progression_GenericSlot, "3"))
                    };

                    // Remove empty slots
                    ProgressionSlots.RemoveWhere(x => x == null);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Reading Jungle Run save data");
                    throw;
                }
            });
        }

        #endregion
    }
}
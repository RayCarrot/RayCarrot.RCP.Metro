using System;
using System.Globalization;
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
    /// View model for the Rayman 3 progression
    /// </summary>
    public class Rayman3ProgressionViewModel : BaseProgressionViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman3ProgressionViewModel() : base(Games.Rayman3)
        {
            // Get the save data directory
            SaveDir = Games.Rayman3.GetInstallDir(false) + "GAMEDATA" + "SaveGame";
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the progression slot view model for the save data from the specified file
        /// </summary>
        /// <param name="filePath">The slot file path</param>
        /// <returns>The progression slot view model</returns>
        protected ProgressionSlotViewModel GetProgressionSlotViewModel(FileSystemPath filePath)
        {
            RCFCore.Logger?.LogInformationSource($"Rayman 3 slot {filePath.Name} is being loaded...");

            // Make sure the file exists
            if (!filePath.FileExists)
            {
                RCFCore.Logger?.LogInformationSource($"Slot was not loaded due to not being found");

                return null;
            }

            // Deserialize and return the data
            var saveData = new Rayman3SaveDataSerializer().Deserialize(filePath);

            RCFCore.Logger?.LogInformationSource($"Slot has been deserialized");

            var formatInfo = new NumberFormatInfo()
            {
                NumberGroupSeparator = " ",
                NumberDecimalDigits = 0
            };

            // TODO: Finish & localize (need to create generators for the names)
            // Create the collection with items for each time trial level + general information
            var progressItems = new ProgressionInfoItemViewModel[]
            {
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Cage, $"{saveData.TotalCages}/60"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"Total: {saveData.TotalScore.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"The Fairy Council: {saveData.Levels[0].Score.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"Clearleaf Forest: {saveData.Levels[1].Score.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"The Bog of Murk: {saveData.Levels[2].Score.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"The Land of the Livid Dead: {saveData.Levels[3].Score.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"The Desert of the Knaaren: {saveData.Levels[4].Score.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"The Longest Shortcut: {saveData.Levels[5].Score.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"The Summit Beyond the Clouds: {saveData.Levels[6].Score.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"Hoodlum Headquarters: {saveData.Levels[7].Score.ToString("n", formatInfo)}"),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, $"The Tower of the Leptys: {saveData.Levels[8].Score.ToString("n", formatInfo)}")
            };

            RCFCore.Logger?.LogInformationSource($"General progress info has been set");

            // Return the data with the collection
            return new Rayman3ProgressionSlotViewModel(() => $"{filePath.RemoveFileExtension().Name}", progressItems, filePath, this);
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        /// <returns>The task</returns>
        public override async Task LoadDataAsync()
        {
            RCFCore.Logger?.LogInformationSource($"Progression data for Rayman 3 is being loaded...");

            // Run on a new thread
            await Task.Run(() =>
            {
                try
                {
                    // Dispose existing slot view models
                    ProgressionSlots.ForEach(x => x.Dispose());

                    RCFCore.Logger?.LogDebugSource($"Existing slots have been disposed");
                    
                    // Clear the collection
                    ProgressionSlots.Clear();

                    // Read and set slot data
                    ProgressionSlots.AddRange(Directory.GetFiles(SaveDir, "*.sav", SearchOption.TopDirectoryOnly).Select(x => GetProgressionSlotViewModel(x)));

                    RCFCore.Logger?.LogInformationSource($"Slots have been loaded");

                    // Remove empty slots
                    ProgressionSlots.RemoveWhere(x => x == null);

                    RCFCore.Logger?.LogDebugSource($"Empty slots have been removed");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Reading Rayman 3 save data");
                    throw;
                }
            });
        }

        #endregion
    }
}
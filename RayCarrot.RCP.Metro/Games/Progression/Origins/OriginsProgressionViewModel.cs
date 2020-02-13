using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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

            // Deserialize and return the data
            var saveData = OriginsPCSaveData.GetSerializer().Deserialize(filePath).SaveData;

            RCFCore.Logger?.LogInformationSource($"Slot has been deserialized");

            // Create the collection with items for each time trial level + general information
            var progressItems = new List<ProgressionInfoItemViewModel>();

            // Create the number format info to use
            var formatInfo = new NumberFormatInfo()
            {
                NumberGroupSeparator = " ",
                NumberDecimalDigits = 0
            };

            // Get the number of Electoons
            var electoons = saveData.Levels.Select(x => x.Value.Object.CageMapPassedDoors.Count).Sum();
            var teeth = saveData.Levels.Select(x => x.Value.Object.ISDs.Select(x => x.Value.Object.TakenTooth.Count)).SelectMany(x => x).Sum();

            // Set general progress info
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RO_Electoon, new LocalizedString(() => $"{electoons}/256")));
            progressItems.Add(new ProgressionInfoItemViewModel(ProgressionIcons.RO_RedTooth, new LocalizedString(() => $"{teeth}/10")));

            // TODO: Set time trials trophies, Lum medals

            RCFCore.Logger?.LogInformationSource($"General progress info has been set");

            // TODO: Add other things to here
            // Calculate the percentage
            var percentage = ((electoons / 256d * 100)).ToString("0.##");

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
            ProgressionSlots.Add(GetProgressionSlotViewModel(SaveDir + "Savegame_1", () => String.Format(Resources.Progression_GenericSlot, "1")));
            ProgressionSlots.Add(GetProgressionSlotViewModel(SaveDir + "Savegame_2", () => String.Format(Resources.Progression_GenericSlot, "1")));
        }

        #endregion
    }
}
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
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
            var saveData = Rayman3PCSaveData.GetSerializer().Deserialize(filePath);

            RCFCore.Logger?.LogInformationSource($"Slot has been deserialized");

            var formatInfo = new NumberFormatInfo()
            {
                NumberGroupSeparator = " ",
                NumberDecimalDigits = 0
            };

            // Create the collection with items for each time trial level + general information
            var progressItems = new ProgressionInfoItemViewModel[]
            {
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Cage, new LocalizedString(() => $"{saveData.TotalCages}/60")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_TotalHeader}: {saveData.TotalScore.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level1Header}: {saveData.Levels[0].Score.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level2Header}: {saveData.Levels[1].Score.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level3Header}: {saveData.Levels[2].Score.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level4Header}: {saveData.Levels[3].Score.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level5Header}: {saveData.Levels[4].Score.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level6Header}: {saveData.Levels[5].Score.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level7Header}: {saveData.Levels[6].Score.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level8Header}: {saveData.Levels[7].Score.ToString("n", formatInfo)}")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R3_Score, new LocalizedString(() => $"{Resources.Progression_R3_Level9Header}: {saveData.Levels[8].Score.ToString("n", formatInfo)}"))
            };

            RCFCore.Logger?.LogInformationSource($"General progress info has been set");

            // Return the data with the collection
            return new Rayman3ProgressionSlotViewModel(new LocalizedString(() => $"{filePath.RemoveFileExtension().Name}"), progressItems, filePath, this);
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        protected override void LoadData()
        {
            // Read and set slot data
            ProgressionSlots.AddRange(Directory.GetFiles(SaveDir, "*.sav", SearchOption.TopDirectoryOnly).Select(x => GetProgressionSlotViewModel(x)));
        }

        #endregion
    }
}
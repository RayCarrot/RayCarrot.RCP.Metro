using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using RayCarrot.UI;
using System.IO;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 1 progression
    /// </summary>
    public class Rayman1ProgressionViewModel : BaseProgressionViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman1ProgressionViewModel() : base(Games.Rayman3)
        {
            // Get the save data directory
            SaveDir = Games.Rayman1.GetInstallDir(false);
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
            RL.Logger?.LogInformationSource($"Rayman 1 slot {filePath.Name} is being loaded...");

            // Make sure the file exists
            if (!filePath.FileExists)
            {
                RL.Logger?.LogInformationSource($"Slot was not loaded due to not being found");

                return null;
            }

            // Open the file in a stream
            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);

            // Create a memory stream
            using var memStream = new MemoryStream();

            // Decode the data
            new Rayman12PCSaveDataEncoder().Decode(fileStream, memStream);

            // Set the position
            memStream.Position = 0;

            // Deserialize and return the data
            var saveData = BinarySerializableHelpers.ReadFromStream<Rayman1PCSaveData>(memStream, Ray1Settings.GetDefaultSettings(Ray1Game.Rayman1, Platform.PC), RCPServices.App.GetBinarySerializerLogger());

            RL.Logger?.LogInformationSource($"Slot has been deserialized");

            // Get total amount of cages
            var cages = saveData.Wi_Save_Zone.Sum(x => x.Cages);

            // Create the collection with items for cages + lives
            var progressItems = new ProgressionInfoItemViewModel[]
            {
                new ProgressionInfoItemViewModel(ProgressionIcons.R1_Cage, new LocalizedString(() => $"{cages}/102")),
                new ProgressionInfoItemViewModel(ProgressionIcons.R1_Life, new LocalizedString(() => $"{saveData.StatusBar[0]}")),
            };

            RL.Logger?.LogInformationSource($"General progress info has been set");

            // Calculate the percentage
            var percentage = ((cages / 102d * 100)).ToString("0.##");

            RL.Logger?.LogInformationSource($"Slot percentage is {percentage}%");

            // Return the data with the collection
            return new Rayman1ProgressionSlotViewModel(new LocalizedString(() => $"{saveData.SaveName} ({percentage}%)"), progressItems, filePath, this);
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        protected override void LoadData()
        {
            // Read and set slot data
            ProgressionSlots.AddRange(Directory.GetFiles(SaveDir, "*.SAV", SearchOption.TopDirectoryOnly).Select(x => GetProgressionSlotViewModel(x)));
        }

        #endregion
    }
}
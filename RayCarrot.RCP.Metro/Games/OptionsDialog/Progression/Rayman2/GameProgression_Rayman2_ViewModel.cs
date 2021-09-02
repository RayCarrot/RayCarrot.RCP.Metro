using System;
using System.Collections.Generic;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System.IO;
using System.Linq;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 2 progression
    /// </summary>
    public class GameProgression_Rayman2_ViewModel : GameProgression_BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameProgression_Rayman2_ViewModel() : base(Games.Rayman2)
        {
            // Get the save data directory
            SaveDir = Games.Rayman2.GetInstallDir(false) + "Data";

            // Get the paths
            ConfigFilePath = SaveDir + "Options" + "Current.cfg";
            SaveGamePath = SaveDir + "SaveGame";
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current config file path
        /// </summary>
        public FileSystemPath ConfigFilePath { get; }

        /// <summary>
        /// The save game directory path
        /// </summary>
        public FileSystemPath SaveGamePath { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the progression slot view model for the save data from the specified file
        /// </summary>
        /// <param name="filePath">The slot file path</param>
        /// <param name="slotName">The generator for the name of the save slot</param>
        /// <returns>The progression slot view model</returns>
        protected GameProgression_BaseSlotViewModel GetProgressionSlotViewModel(FileSystemPath filePath, string slotName)
        {
            RL.Logger?.LogInformationSource($"Rayman 2 slot {slotName} is being loaded...");

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
            var saveData = BinarySerializableHelpers.ReadFromStream<Rayman2PCSaveData>(memStream, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC), RCPServices.App.GetBinarySerializerLogger(filePath.Name));

            RL.Logger?.LogInformationSource($"Slot has been deserialized");

            // Get the bit array
            var array = saveData.GlobalArrayAsBitFlags();

            // Get total amount of Lums and cages
            var lums =
                array.Skip(0).Take(800).Select(x => x ? 1 : 0).Sum() +
                array.Skip(1200).Take(194).Select(x => x ? 1 : 0).Sum() + 
                // Woods of Light
                array.Skip(1395).Take(5).Select(x => x ? 1 : 0).Sum() + 
                // 1000th Lum
                (array[1013] ? 1 : 0);

            var cages = array.Skip(839).Take(80).Select(x => x ? 1 : 0).Sum();
            var walkOfLifeTime = saveData.GlobalArray[12] * 10;
            var walkOfPowerTime = saveData.GlobalArray[11] * 10;

            // Create the collection with items for cages + lives
            var progressItems = new List<GameProgression_InfoItemViewModel>
            {
                new GameProgression_InfoItemViewModel(GameProgression_Icon.R2_Lum, new LocalizedString(() => $"{lums}/1000")),
                new GameProgression_InfoItemViewModel(GameProgression_Icon.R2_Cage, new LocalizedString(() => $"{cages}/80")),
            };

            if (walkOfLifeTime > 120)
                progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.R2_Clock, new LocalizedString(() => $"{new TimeSpan(0, 0, 0, 0, walkOfLifeTime):mm\\:ss\\:ff}"), new LocalizedString(() => Resources.R2_BonusLevelName_1)));

            if (walkOfPowerTime > 120)
                progressItems.Add(new GameProgression_InfoItemViewModel(GameProgression_Icon.R2_Clock, new LocalizedString(() => $"{new TimeSpan(0, 0, 0, 0, walkOfPowerTime):mm\\:ss\\:ff}"), new LocalizedString(() => Resources.R2_BonusLevelName_2)));

            RL.Logger?.LogInformationSource($"General progress info has been set");

            // Get the name and percentage
            var separatorIndex = slotName.LastIndexOf((char)0x20);
            var name = slotName.Substring(0, separatorIndex);
            var percentage = slotName.Substring(separatorIndex + 1);

            RL.Logger?.LogInformationSource($"Slot percentage is {percentage}%");

            // Return the data with the collection
            return new GameProgression_Rayman2_SlotViewModel(new LocalizedString(() => $"{name} ({percentage}%)"), progressItems.ToArray(), filePath, this);
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Loads the current save data if available
        /// </summary>
        protected override void LoadData()
        {
            // Make sure the file exists
            if (!ConfigFilePath.FileExists)
                return;

            // Create streams
            using var saveFileStream = File.OpenRead(ConfigFilePath);
            using var decodedDataStream = new MemoryStream();

            // Decode the save file
            new Rayman12PCSaveDataEncoder().Decode(saveFileStream, decodedDataStream);

            // Set position to 0
            decodedDataStream.Position = 0;

            // Get the serialized data
            var config = BinarySerializableHelpers.ReadFromStream<Rayman2PCConfigData>(decodedDataStream, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC), RCPServices.App.GetBinarySerializerLogger(ConfigFilePath.Name));

            // Read and set slot data
            ProgressionSlots.AddRange(config.Slots.Select(x => GetProgressionSlotViewModel(SaveGamePath + $"Slot{x.SlotIndex}" + "General.sav", x.SlotDisplayName)));
        }

        #endregion
    }
}
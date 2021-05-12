using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a Origins progression slot item
    /// </summary>
    public class OriginsProgressionSlotViewModel : ProgressionSlotViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="slotName">The slot name</param>
        /// <param name="items">The progression info items</param>
        /// <param name="saveSlotFilePath">The file path for the save slot</param>
        /// <param name="progressionViewModel">The progression view model containing this slot</param>
        public OriginsProgressionSlotViewModel(LocalizedString slotName, ProgressionInfoItemViewModel[] items, FileSystemPath saveSlotFilePath, BaseProgressionViewModel progressionViewModel) : base(slotName, items, saveSlotFilePath, progressionViewModel)
        { }

        // TODO: Enable this again - currently Origins doesn't load modified saves, possibly due to a checksum check
        /// <summary>
        /// Indicates if the slot can be exported/imported
        /// </summary>
        public override bool CanModify => false;

        /// <summary>
        /// Exports the save slot from the specified path
        /// </summary>
        /// <param name="outputFilePath">The output file path</param>
        /// <returns>The task</returns>
        protected override Task ExportSaveDataAsync(FileSystemPath outputFilePath)
        {
            // Get the serialized level data
            var data = BinarySerializableHelpers.ReadFromFile<OriginsPCSaveData>(SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanOrigins, Platform.PC), RCPServices.App.GetBinarySerializerLogger(SaveSlotFilePath.Name));

            // Export the data
            JsonHelpers.SerializeToFile(data, outputFilePath);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Imports an exported save slot to the save slot from the specified path
        /// </summary>
        /// <param name="inputFilePath">The input file path</param>
        /// <returns>The task</returns>
        protected override Task ImportSaveDataAsync(FileSystemPath inputFilePath)
        {
            // Get the serialized data
            var data = BinarySerializableHelpers.ReadFromFile<OriginsPCSaveData>(SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanOrigins, Platform.PC), RCPServices.App.GetBinarySerializerLogger(SaveSlotFilePath.Name));

            // Deserialize the input data
            data.SaveData = JsonHelpers.DeserializeFromFile<OriginsPCSaveData.PersistentGameData_Universe>(inputFilePath);

            // Import the data
            BinarySerializableHelpers.WriteToFile<OriginsPCSaveData>(data, SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanOrigins, Platform.PC), RCPServices.App.GetBinarySerializerLogger(SaveSlotFilePath.Name));

            return Task.CompletedTask;
        }
    }
}
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a Rayman 2 progression slot item
    /// </summary>
    public class GameProgression_Rayman2_SlotViewModel : GameProgression_BaseSlotViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="slotName">The slot name</param>
        /// <param name="items">The progression info items</param>
        /// <param name="saveSlotFilePath">The file path for the save slot</param>
        /// <param name="progressionViewModel">The progression view model containing this slot</param>
        public GameProgression_Rayman2_SlotViewModel(LocalizedString slotName, GameProgression_InfoItemViewModel[] items, FileSystemPath saveSlotFilePath, GameProgression_BaseViewModel progressionViewModel) : base(slotName, items, saveSlotFilePath, progressionViewModel)
        {

        }

        /// <summary>
        /// Indicates if the slot can be exported/imported
        /// </summary>
        public override bool CanModify => true;

        /// <summary>
        /// Exports the save slot from the specified path
        /// </summary>
        /// <param name="outputFilePath">The output file path</param>
        /// <returns>The task</returns>
        protected override Task ExportSaveDataAsync(FileSystemPath outputFilePath)
        {
            // Create streams
            using var saveFileStream = File.OpenRead(SaveSlotFilePath);
            using var decodedDataStream = new MemoryStream();

            // Decode the save file
            new Rayman12PCSaveDataEncoder().Decode(saveFileStream, decodedDataStream);

            // Set position to 0
            decodedDataStream.Position = 0;

            // Get the serialized data
            var data = BinarySerializableHelpers.ReadFromStream<Rayman2PCSaveData>(decodedDataStream, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC), Services.App.GetBinarySerializerLogger(SaveSlotFilePath.Name));

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
            // Deserialize the input data
            var data = JsonHelpers.DeserializeFromFile<Rayman2PCSaveData>(inputFilePath);

            // Create streams
            using var decodedDataStream = new MemoryStream();
            using var saveFileStream = File.Create(SaveSlotFilePath);

            // Import the data
            BinarySerializableHelpers.WriteToStream(data, decodedDataStream, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC), Services.App.GetBinarySerializerLogger(SaveSlotFilePath.Name));

            // Set position to 0
            decodedDataStream.Position = 0;

            // Encode the data to the file
            new Rayman12PCSaveDataEncoder().Encode(decodedDataStream, saveFileStream);

            return Task.CompletedTask;
        }
    }
}
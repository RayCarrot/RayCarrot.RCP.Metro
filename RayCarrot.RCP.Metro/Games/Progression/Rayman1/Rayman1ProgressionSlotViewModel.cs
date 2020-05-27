using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a Rayman 1 progression slot item
    /// </summary>
    public class Rayman1ProgressionSlotViewModel : ProgressionSlotViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="slotName">The slot name</param>
        /// <param name="items">The progression info items</param>
        /// <param name="saveSlotFilePath">The file path for the save slot</param>
        /// <param name="progressionViewModel">The progression view model containing this slot</param>
        public Rayman1ProgressionSlotViewModel(LocalizedString slotName, ProgressionInfoItemViewModel[] items, FileSystemPath saveSlotFilePath, BaseProgressionViewModel progressionViewModel) : base(slotName, items, saveSlotFilePath, progressionViewModel)
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
            var data = BinarySerializableHelpers.ReadFromStream<Rayman1PCSaveData>(decodedDataStream, Ray1Settings.GetDefaultSettings(), RCPServices.App.GetBinarySerializerLogger());

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
            var data = JsonHelpers.DeserializeFromFile<Rayman1PCSaveData>(inputFilePath);

            // Create streams
            using var decodedDataStream = new MemoryStream();
            using var saveFileStream = File.Create(SaveSlotFilePath);

            // Import the data
            BinarySerializableHelpers.WriteToStream(data, decodedDataStream, Ray1Settings.GetDefaultSettings(), RCPServices.App.GetBinarySerializerLogger());

            // Set position to 0
            decodedDataStream.Position = 0;

            // Encode the data to the file
            new Rayman12PCSaveDataEncoder().Encode(decodedDataStream, saveFileStream);

            return Task.CompletedTask;
        }
    }
}
using Newtonsoft.Json;
using RayCarrot.IO;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a Legends progression slot item
    /// </summary>
    public class LegendsProgressionSlotViewModel : ProgressionSlotViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="slotName">The slot name</param>
        /// <param name="items">The progression info items</param>
        /// <param name="saveSlotFilePath">The file path for the save slot</param>
        /// <param name="progressionViewModel">The progression view model containing this slot</param>
        public LegendsProgressionSlotViewModel(LocalizedString slotName, ProgressionInfoItemViewModel[] items, FileSystemPath saveSlotFilePath, BaseProgressionViewModel progressionViewModel) : base(slotName, items, saveSlotFilePath, progressionViewModel)
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
            // Get the serialized level data
            var data = JsonConvert.SerializeObject(LegendsPCSaveData.GetSerializer().Deserialize(SaveSlotFilePath).SaveData, Formatting.Indented);

            // Export the data
            File.WriteAllText(outputFilePath, data);

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
            var data = LegendsPCSaveData.GetSerializer().Deserialize(SaveSlotFilePath);

            // Deserialize the input data
            data.SaveData = JsonConvert.DeserializeObject<LegendsPCSaveData.PersistentGameData_Universe>(File.ReadAllText(inputFilePath));

            // Import the data
            LegendsPCSaveData.GetSerializer().Serialize(SaveSlotFilePath, data);

            return Task.CompletedTask;
        }
    }
}
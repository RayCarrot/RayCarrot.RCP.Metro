using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a Jungle Run progression slot item
    /// </summary>
    public class JungleRunProgressionSlotViewModel : ProgressionSlotViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="slotNameGenerator">The function to get the slot name</param>
        /// <param name="items">The progression info items</param>
        /// <param name="saveSlotFilePath">The file path for the save slot</param>
        /// <param name="progressionViewModel">The progression view model containing this slot</param>
        public JungleRunProgressionSlotViewModel(Func<string> slotNameGenerator, ProgressionInfoItemViewModel[] items, FileSystemPath saveSlotFilePath, BaseProgressionViewModel progressionViewModel) : base(slotNameGenerator, items, saveSlotFilePath, progressionViewModel)
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
            var data = JsonConvert.SerializeObject(new JungleRunSaveDataSerializer().Deserialize(SaveSlotFilePath).Levels, Formatting.Indented);

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
            var data = new JungleRunSaveDataSerializer().Deserialize(SaveSlotFilePath);

            // Deserialize the input data
            var inputData = JsonConvert.DeserializeObject<JungleRunSaveDataLevelCollection>(File.ReadAllText(inputFilePath));

            // Update the data
            data.Levels = inputData;

            // Import the data
            new JungleRunSaveDataSerializer().Serialize(SaveSlotFilePath, data);

            return Task.CompletedTask;
        }
    }
}
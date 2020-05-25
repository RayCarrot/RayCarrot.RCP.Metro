﻿using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System.Threading.Tasks;

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
            var data = BinarySerializableHelpers.ReadFromFile<LegendsPCSaveData>(SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanLegends, Platform.PC), RCPServices.App.GetBinarySerializerLogger()).SaveData;

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
            var data = BinarySerializableHelpers.ReadFromFile<LegendsPCSaveData>(SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanLegends, Platform.PC), RCPServices.App.GetBinarySerializerLogger());

            // Deserialize the input data
            data.SaveData = JsonHelpers.DeserializeFromFile<LegendsPCSaveData.PersistentGameData_Universe>(inputFilePath);

            // Import the data
            BinarySerializableHelpers.WriteToFile(data, SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanLegends, Platform.PC), RCPServices.App.GetBinarySerializerLogger());

            return Task.CompletedTask;
        }
    }
}
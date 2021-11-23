using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a Rayman M/Arena progression slot item
/// </summary>
public class GameProgression_RaymanMArena_SlotViewModel : GameProgression_BaseSlotViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="slotName">The slot name</param>
    /// <param name="items">The progression info items</param>
    /// <param name="saveSlotFilePath">The file path for the save slot</param>
    /// <param name="progressionViewModel">The progression view model containing this slot</param>
    public GameProgression_RaymanMArena_SlotViewModel(LocalizedString slotName, GameProgression_InfoItemViewModel[] items, FileSystemPath saveSlotFilePath, GameProgression_BaseViewModel progressionViewModel) : base(slotName, items, saveSlotFilePath, progressionViewModel)
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
        // Get the serialized data
        var data = BinarySerializableHelpers.ReadFromFile<RaymanMPCSaveData>(SaveSlotFilePath, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.RaymanM, Platform.PC), Services.App.GetBinarySerializerLogger(SaveSlotFilePath));

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
        var data = JsonHelpers.DeserializeFromFile<RaymanMPCSaveData>(inputFilePath);

        // Import the data
        BinarySerializableHelpers.WriteToFile(data, SaveSlotFilePath, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.RaymanM, Platform.PC), Services.App.GetBinarySerializerLogger(SaveSlotFilePath.Name));

        return Task.CompletedTask;
    }
}
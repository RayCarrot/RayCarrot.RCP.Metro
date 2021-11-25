#nullable disable
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a Jungle Run progression slot item
/// </summary>
public class GameProgression_JungleRun_SlotViewModel : GameProgression_BaseSlotViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="slotName">The slot name</param>
    /// <param name="items">The progression info items</param>
    /// <param name="saveSlotFilePath">The file path for the save slot</param>
    /// <param name="progressionViewModel">The progression view model containing this slot</param>
    public GameProgression_JungleRun_SlotViewModel(LocalizedString slotName, GameProgression_InfoItemViewModel[] items, FileSystemPath saveSlotFilePath, GameProgression_BaseViewModel progressionViewModel) : base(slotName, items, saveSlotFilePath, progressionViewModel)
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
        var data = BinarySerializableHelpers.ReadFromFile<JungleRunPCSaveData>(SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanJungleRun, Platform.PC), Services.App.GetBinarySerializerLogger(SaveSlotFilePath.Name)).Levels;

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
        var data = BinarySerializableHelpers.ReadFromFile<JungleRunPCSaveData>(SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanJungleRun, Platform.PC), Services.App.GetBinarySerializerLogger(SaveSlotFilePath.Name));

        // Deserialize the input data
        data.Levels = JsonHelpers.DeserializeFromFile<JungleRunPCSaveDataLevel[]>(inputFilePath);

        // Import the data
        BinarySerializableHelpers.WriteToFile(data, SaveSlotFilePath, UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanJungleRun, Platform.PC), Services.App.GetBinarySerializerLogger(SaveSlotFilePath.Name));

        return Task.CompletedTask;
    }
}
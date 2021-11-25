#nullable disable
using System;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using RayCarrot.UI;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman 3 progression
/// </summary>
public class GameProgression_Rayman3_ViewModel : GameProgression_BaseViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public GameProgression_Rayman3_ViewModel() : base(Games.Rayman3)
    {
        // Get the save data directory
        SaveDir = Games.Rayman3.GetInstallDir(false) + "GAMEDATA" + "SaveGame";
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Protected Methods

    /// <summary>
    /// Get the progression slot view model for the save data from the specified file
    /// </summary>
    /// <param name="filePath">The slot file path</param>
    /// <returns>The progression slot view model</returns>
    protected GameProgression_BaseSlotViewModel GetProgressionSlotViewModel(FileSystemPath filePath)
    {
        Logger.Info("Rayman 3 slot {0} is being loaded...", filePath.Name);

        // Make sure the file exists
        if (!filePath.FileExists)
        {
            Logger.Info("Slot was not loaded due to not being found");

            return null;
        }

        // Open the file in a stream
        using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);

        // Create a memory stream
        using var memStream = new MemoryStream();

        // Decode the data
        new Rayman3SaveDataEncoder().Decode(fileStream, memStream);

        // Set the position
        memStream.Position = 0;

        // Deserialize the data
        var saveData = BinarySerializableHelpers.ReadFromStream<Rayman3PCSaveData>(memStream, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman3, Platform.PC), Services.App.GetBinarySerializerLogger(filePath.Name));

        Logger.Info("Slot has been deserialized");

        var formatInfo = new NumberFormatInfo()
        {
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 0
        };

        // Create the collection with items for each level + general information
        var progressItems = new GameProgression_InfoItemViewModel[]
        {
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Cage, new ConstLocString($"{saveData.TotalCages}/60")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_TotalHeader}: {saveData.TotalScore.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level1Header}: {saveData.Levels[0].Score.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level2Header}: {saveData.Levels[1].Score.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level3Header}: {saveData.Levels[2].Score.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level4Header}: {saveData.Levels[3].Score.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level5Header}: {saveData.Levels[4].Score.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level6Header}: {saveData.Levels[5].Score.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level7Header}: {saveData.Levels[6].Score.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level8Header}: {saveData.Levels[7].Score.ToString("n", formatInfo)}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R3_Score, new GeneratedLocString(() => $"{Resources.Progression_R3_Level9Header}: {saveData.Levels[8].Score.ToString("n", formatInfo)}"))
        };

        Logger.Info("General progress info has been set");

        // Return the data with the collection
        return new GameProgression_Rayman3_SlotViewModel(new ConstLocString($"{filePath.RemoveFileExtension().Name}"), progressItems, filePath, this);
    }

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Loads the current save data if available
    /// </summary>
    protected override void LoadData()
    {
        // Read and set slot data
        ProgressionSlots.AddRange(Directory.GetFiles(SaveDir, "*.sav", SearchOption.TopDirectoryOnly).Select(x => GetProgressionSlotViewModel(x)));
    }

    #endregion
}
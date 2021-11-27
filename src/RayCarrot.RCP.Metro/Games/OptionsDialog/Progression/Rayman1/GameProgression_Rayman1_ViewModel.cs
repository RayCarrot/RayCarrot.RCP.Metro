#nullable disable
using RayCarrot.Binary;
using RayCarrot.IO;
using NLog;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using System.IO;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman 1 progression
/// </summary>
public class GameProgression_Rayman1_ViewModel : GameProgression_BaseViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public GameProgression_Rayman1_ViewModel() : base(Games.Rayman1)
    {
        // Get the save data directory
        SaveDir = Games.Rayman1.GetInstallDir(false);
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
        Logger.Info("Rayman 1 slot {0} is being loaded...", filePath.Name);

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
        new Rayman12PCSaveDataEncoder().Decode(fileStream, memStream);

        // Set the position
        memStream.Position = 0;

        // Deserialize and return the data
        var saveData = BinarySerializableHelpers.ReadFromStream<Rayman1PCSaveData>(memStream, Ray1Settings.GetDefaultSettings(Ray1Game.Rayman1, Platform.PC), Services.App.GetBinarySerializerLogger(filePath.Name));

        Logger.Info("Slot has been deserialized");

        // Get total amount of cages
        var cages = saveData.Wi_Save_Zone.Sum(x => x.Cages);

        // Create the collection with items for cages + lives
        var progressItems = new GameProgression_InfoItemViewModel[]
        {
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R1_Cage, new ConstLocString($"{cages}/102")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R1_Continue, new ConstLocString($"{saveData.ContinuesCount}")),
            new GameProgression_InfoItemViewModel(GameProgression_Icon.R1_Life, new ConstLocString($"{saveData.StatusBar.LivesCount}")),
        };

        Logger.Info("General progress info has been set");

        // Calculate the percentage
        var percentage = ((cages / 102d * 100)).ToString("0.##");

        Logger.Info("Slot percentage is {0}%", percentage);

        // Return the data with the collection
        return new GameProgression_Rayman1_SlotViewModel(new ConstLocString($"{saveData.SaveName.ToUpper()} ({percentage}%)"), progressItems, filePath, this);
    }

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Loads the current save data if available
    /// </summary>
    protected override void LoadData()
    {
        // Read and set slot data
        ProgressionSlots.AddRange(Directory.GetFiles(SaveDir, "*.SAV", SearchOption.TopDirectoryOnly).Select(x => GetProgressionSlotViewModel(x)));
    }

    #endregion
}
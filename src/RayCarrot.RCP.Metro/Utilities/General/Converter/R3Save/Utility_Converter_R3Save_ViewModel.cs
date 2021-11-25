#nullable disable
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Utility view model for converting Rayman 3 .sav files
/// </summary>
public class Utility_Converter_R3Save_ViewModel : Utility_BaseConverter_ViewModel<Platform>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Converter_R3Save_ViewModel()
    {
        GameModeSelection = new EnumSelectionViewModel<Platform>(Platform.PC, new Platform[]
        {
            Platform.PC
        });
    }

    #endregion

    #region Public Override Properties

    /// <summary>
    /// The game mode selection
    /// </summary>
    public override EnumSelectionViewModel<Platform> GameModeSelection { get; }

    #endregion

    #region Public Override Methods

    /// <summary>
    /// Converts from the format
    /// </summary>
    /// <returns>The task</returns>
    public override async Task ConvertFromAsync()
    {
        var settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman3, GameModeSelection.SelectedValue);

        await ConvertFromAsync<Rayman3PCSaveData>(settings, (data, filePath) =>
        {
            // Save the data
            SerializeJSON(data, filePath);
        }, new FileFilterItem("*.sav", "SAV").ToString(), new[]
        {
            ".json"
        }, Games.Rayman3.GetInstallDir(false), new Rayman3SaveDataEncoder());
    }

    /// <summary>
    /// Converts to the format
    /// </summary>
    /// <returns>The task</returns>
    public override async Task ConvertToAsync()
    {
        var settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman3, GameModeSelection.SelectedValue);

        await ConvertToAsync(settings, (filePath, format) =>
        {
            // Read the data
            return DeserializeJSON<Rayman3PCSaveData>(filePath);
        }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".sav"), null, new Rayman3SaveDataEncoder());
    }

    #endregion
}
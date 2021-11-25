#nullable disable
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Utility view model for converting Rayman 1 .sav files
/// </summary>
public class Utility_Converter_R1Save_ViewModel : Utility_BaseConverter_ViewModel<Platform>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Converter_R1Save_ViewModel()
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
        var settings = Ray1Settings.GetDefaultSettings(Ray1Game.Rayman1, Platform.PC);

        await ConvertFromAsync<Rayman1PCSaveData>(settings, (data, filePath) =>
        {
            // Save the data
            SerializeJSON(data, filePath);
        }, new FileFilterItem("*.SAV", "SAV").ToString(), new[]
        {
            ".json"
        }, Games.Rayman1.GetInstallDir(false), new Rayman12PCSaveDataEncoder());
    }

    /// <summary>
    /// Converts to the format
    /// </summary>
    /// <returns>The task</returns>
    public override async Task ConvertToAsync()
    {
        var settings = Ray1Settings.GetDefaultSettings(Ray1Game.Rayman1, Platform.PC);

        await ConvertToAsync(settings, (filePath, format) =>
        {
            // Read the data
            return DeserializeJSON<Rayman1PCSaveData>(filePath);
        }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".SAV"), null, new Rayman12PCSaveDataEncoder());
    }

    #endregion
}
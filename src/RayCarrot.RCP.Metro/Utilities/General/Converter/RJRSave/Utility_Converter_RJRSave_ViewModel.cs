﻿#nullable disable
using System;
using System.Threading.Tasks;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Utility view model for converting Rayman Jungle Run save files
/// </summary>
public class Utility_Converter_RJRSave_ViewModel : Utility_BaseConverter_ViewModel<Platform>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Converter_RJRSave_ViewModel()
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
        var settings = UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanJungleRun, GameModeSelection.SelectedValue);

        await ConvertFromAsync<JungleRunPCSaveData>(settings, (data, filePath) =>
        {
            // Save the data
            SerializeJSON(data, filePath);
        }, new FileFilterItem("*.dat", "DAT").ToString(), new[]
        {
            ".json"
        }, Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + Games.RaymanJungleRun.GetManager<GameManager_WinStore>().FullPackageName + "LocalState");
    }

    /// <summary>
    /// Converts to the format
    /// </summary>
    /// <returns>The task</returns>
    public override async Task ConvertToAsync()
    {
        var settings = UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanJungleRun, GameModeSelection.SelectedValue);

        await ConvertToAsync<JungleRunPCSaveData>(settings, (filePath, format) =>
        {
            // Read the data
            return DeserializeJSON<JungleRunPCSaveData>(filePath);
        }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".dat"));
    }

    #endregion
}
﻿using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using RayCarrot.UI;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman 3 .sav files
    /// </summary>
    public class R3SaveConverterUtilityViewModel : BaseConverterUtilityViewModel<UtilityPlatforms>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R3SaveConverterUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<UtilityPlatforms>(UtilityPlatforms.PC, new UtilityPlatforms[]
            {
                UtilityPlatforms.PC
            });
        }

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<UtilityPlatforms> GameModeSelection { get; }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Converts from the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertFromAsync()
        {
            var settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman3, 
                // IDEA: Allow this to be changed once we allow more platforms
                OpenSpacePlatform.PC);

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
            var settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman3,
                // IDEA: Allow this to be changed once we allow more platforms
                OpenSpacePlatform.PC);

            await ConvertToAsync(settings, (filePath, format) =>
            {
                // Read the data
                return DeserializeJSON<Rayman3PCSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".sav"), null, new Rayman3SaveDataEncoder());
        }

        #endregion
    }
}
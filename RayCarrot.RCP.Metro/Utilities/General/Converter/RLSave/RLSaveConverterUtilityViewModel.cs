using System;
using System.Threading.Tasks;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman Legends save files
    /// </summary>
    public class RLSaveConverterUtilityViewModel : BaseConverterUtilityViewModel<UtilityPlatforms>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RLSaveConverterUtilityViewModel()
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
            var attr = GameModeSelection.SelectedValue.GetAttribute<UbiArtGameModeInfoAttribute>();
            var settings = UbiArtSettings.GetDefaultSettings(attr.Game, attr.Platform);

            await ConvertFromAsync<LegendsPCSaveData>(settings, (data, filePath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*", String.Empty).ToString(), new[]
            {
                ".json"
            }, Environment.SpecialFolder.MyDocuments.GetFolderPath() + "Rayman Legends");
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            var attr = GameModeSelection.SelectedValue.GetAttribute<UbiArtGameModeInfoAttribute>();
            var settings = UbiArtSettings.GetDefaultSettings(attr.Game, attr.Platform);

            await ConvertToAsync<LegendsPCSaveData>(settings, (filePath, format) =>
            {
                // Read the data
                return DeserializeJSON<LegendsPCSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(String.Empty));
        }

        #endregion
    }
}
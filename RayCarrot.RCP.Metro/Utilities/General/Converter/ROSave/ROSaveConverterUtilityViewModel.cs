using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman Origins save files
    /// </summary>
    public class ROSaveConverterUtilityViewModel : BaseConverterUtilityViewModel<Platform>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ROSaveConverterUtilityViewModel()
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
            var settings = UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanOrigins, GameModeSelection.SelectedValue);

            await ConvertFromAsync<OriginsPCSaveData>(settings, (data, filePath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*", String.Empty).ToString(), new[]
            {
                ".json"
            }, Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins");
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            var settings = UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanOrigins, GameModeSelection.SelectedValue);

            await ConvertToAsync<OriginsPCSaveData>(settings, (filePath, format) =>
            {
                // Read the data
                return DeserializeJSON<OriginsPCSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(String.Empty));
        }

        #endregion
    }
}
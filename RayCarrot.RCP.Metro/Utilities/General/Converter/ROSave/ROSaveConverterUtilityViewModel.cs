using System;
using System.Threading.Tasks;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman Origins save files
    /// </summary>
    public class ROSaveConverterUtilityViewModel : BaseConverterUtilityViewModel<UtilityPlatforms>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ROSaveConverterUtilityViewModel()
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
            await ConvertFromAsync(OriginsPCSaveData.GetSerializer(), (data, filePath, configPath) =>
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
            await ConvertToAsync(OriginsPCSaveData.GetSerializer(), (filePath, configPath) =>
            {
                // Read the data
                return DeserializeJSON<OriginsPCSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), String.Empty, false);
        }

        #endregion
    }
}
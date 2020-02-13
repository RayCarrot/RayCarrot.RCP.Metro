using System;
using System.Threading.Tasks;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman Jungle Run save files
    /// </summary>
    public class RJRSaveConverterUtilityViewModel : BaseConverterUtilityViewModel<UtilityPlatforms>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RJRSaveConverterUtilityViewModel()
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
            await ConvertFromAsync(JungleRunPCSaveData.GetSerializer(), (data, filePath, configPath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.dat", "DAT").ToString(), new[]
            {
                ".json"
            }, Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + Games.RaymanJungleRun.GetManager<RCPWinStoreGame>().FullPackageName + "LocalState");
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            await ConvertToAsync(JungleRunPCSaveData.GetSerializer(), (filePath, configPath) =>
            {
                // Read the data
                return DeserializeJSON<JungleRunPCSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), ".dat", false);
        }

        #endregion
    }
}
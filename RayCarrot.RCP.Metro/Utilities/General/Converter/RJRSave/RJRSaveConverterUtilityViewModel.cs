using System.Threading.Tasks;
using RayCarrot.IO;
using RayCarrot.Rayman;

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
            await ConvertFromAsync(new JungleRunSaveDataSerializer(), (data, filePath, configPath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.dat", "DAT").ToString(), new[]
            {
                ".json"
            }, Games.RaymanJungleRun);
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            await ConvertToAsync(new JungleRunSaveDataSerializer(), (filePath, configPath) =>
            {
                // Read the data
                return DeserializeJSON<JungleRunPCSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), ".dat", false);
        }

        #endregion
    }
}
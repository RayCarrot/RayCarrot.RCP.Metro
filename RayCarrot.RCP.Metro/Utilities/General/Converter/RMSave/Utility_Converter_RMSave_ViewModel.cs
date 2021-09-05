using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman M/Arena .sav files
    /// </summary>
    public class Utility_Converter_RMSave_ViewModel : Utility_BaseConverter_ViewModel<GameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Utility_Converter_RMSave_ViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.RaymanMPC, new GameMode[]
            {
                GameMode.RaymanMPC,
                GameMode.RaymanArenaPC,
            });
        }

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Converts from the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertFromAsync()
        {
            var attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();
            var settings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

            await ConvertFromAsync<RaymanMPCSaveData>(settings, (data, filePath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.sav", "SAV").ToString(), new[]
            {
                ".json"
            }, GameModeSelection.SelectedValue.GetGame()?.GetInstallDir(false));
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            var attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();
            var settings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

            await ConvertToAsync(settings, (filePath, format) =>
            {
                // Read the data
                return DeserializeJSON<RaymanMPCSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".sav"));
        }

        #endregion
    }
}
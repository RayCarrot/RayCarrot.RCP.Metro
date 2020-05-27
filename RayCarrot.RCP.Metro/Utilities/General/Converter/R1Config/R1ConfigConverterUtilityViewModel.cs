using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Rayman.Ray1;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman 1 .cfg files
    /// </summary>
    public class R1ConfigConverterUtilityViewModel : BaseConverterUtilityViewModel<Ray1GameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1ConfigConverterUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<Ray1GameMode>(Ray1GameMode.Rayman1PC, new[]
            {
                Ray1GameMode.Rayman1PC,
                Ray1GameMode.RayKitPC,
            });
        }

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<Ray1GameMode> GameModeSelection { get; }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Converts from the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertFromAsync()
        {
            var attr = GameModeSelection.SelectedValue.GetAttribute<Ray1GameModeInfoAttribute>();
            var settings = Ray1Settings.GetDefaultSettings(attr.Game, attr.Platform);

            await ConvertFromAsync<Rayman1PCConfigData>(settings, (data, filePath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.cfg", "CFG").ToString(), new[]
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
            var attr = GameModeSelection.SelectedValue.GetAttribute<Ray1GameModeInfoAttribute>();
            var settings = Ray1Settings.GetDefaultSettings(attr.Game, attr.Platform);

            await ConvertToAsync(settings, (filePath, format) =>
            {
                // Read the data
                return DeserializeJSON<Rayman1PCConfigData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".cfg"));
        }

        #endregion
    }
}
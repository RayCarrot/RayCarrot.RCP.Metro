using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman 1 .cfg files
    /// </summary>
    public class R1ConfigConverterUtilityViewModel : BaseConverterUtilityViewModel<Platform>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1ConfigConverterUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<Platform>(Platform.PC, new[]
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
            var settings = Ray1Settings.GetDefaultSettings();

            await ConvertFromAsync<Rayman1PCConfigData>(settings, (data, filePath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.cfg", "CFG").ToString(), new[]
            {
                ".json"
            }, Games.Rayman1.GetInstallDir(false));
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            var settings = Ray1Settings.GetDefaultSettings();

            await ConvertToAsync(settings, (filePath, format) =>
            {
                // Read the data
                return DeserializeJSON<Rayman1PCConfigData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".cfg"));
        }

        #endregion
    }
}
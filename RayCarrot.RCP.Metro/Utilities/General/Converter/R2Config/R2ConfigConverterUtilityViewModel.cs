using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman 2 .cfg files
    /// </summary>
    public class R2ConfigConverterUtilityViewModel : BaseConverterUtilityViewModel<Platform>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R2ConfigConverterUtilityViewModel()
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
            var settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC);

            await ConvertFromAsync<Rayman2PCConfigData>(settings, (data, filePath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.cfg", "CFG").ToString(), new[]
            {
                ".json"
            }, Games.Rayman2.GetInstallDir(false), new Rayman12PCSaveDataEncoder());
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            var settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC);

            await ConvertToAsync(settings, (filePath, format) =>
            {
                // Read the data
                return DeserializeJSON<Rayman2PCConfigData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".cfg"), null, new Rayman12PCSaveDataEncoder());
        }

        #endregion
    }
}
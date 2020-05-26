using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using System.Text;
using System.Threading.Tasks;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman 1 .sav files
    /// </summary>
    public class R1SaveConverterUtilityViewModel : BaseConverterUtilityViewModel<Platform>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1SaveConverterUtilityViewModel()
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
            var settings = new BinarySerializerSettings(Endian.Little, Encoding.GetEncoding(437));

            await ConvertFromAsync<Rayman1PCSaveData>(settings, (data, filePath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.SAV", "SAV").ToString(), new[]
            {
                ".json"
            }, Games.Rayman1.GetInstallDir(false), new Rayman12PCSaveDataEncoder());
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            var settings = new BinarySerializerSettings(Endian.Little, Encoding.GetEncoding(437));

            await ConvertToAsync(settings, (filePath, format) =>
            {
                // Read the data
                return DeserializeJSON<Rayman1PCSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".SAV"), null, new Rayman12PCSaveDataEncoder());
        }

        #endregion
    }
}
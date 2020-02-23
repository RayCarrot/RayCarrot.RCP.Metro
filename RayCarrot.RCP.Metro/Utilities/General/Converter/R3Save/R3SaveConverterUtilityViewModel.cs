using RayCarrot.IO;
using RayCarrot.UI;
using System.Threading.Tasks;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting Rayman 3 .sav files
    /// </summary>
    public class R3SaveConverterUtilityViewModel : BaseConverterUtilityViewModel<UtilityPlatforms>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R3SaveConverterUtilityViewModel()
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
            await ConvertFromAsync(Rayman3PCSaveData.GetSerializer(), (data, filePath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.sav", "SAV").ToString(), new[]
            {
                ".json"
            }, Games.Rayman3.GetInstallDir(false), new Rayman3SaveDataEncoder());
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            // TODO: Implement
            await RCFUI.MessageUI.DisplayMessageAsync(Resources.NotImplemented, MessageType.Information);
        }

        #endregion
    }
}
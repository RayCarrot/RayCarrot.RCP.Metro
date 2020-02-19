using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using System.Text;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting .loc and .loc8 files
    /// </summary>
    public class LOCConverterUtilityViewModel : BaseConverterUtilityViewModel<UbiArtGameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LOCConverterUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<UbiArtGameMode>(UbiArtGameMode.RaymanOriginsPC, new[]
            {
                UbiArtGameMode.RaymanOriginsPC,
                UbiArtGameMode.RaymanFiestaRunPC,
                UbiArtGameMode.RaymanFiestaRunAndroid,
                UbiArtGameMode.RaymanLegendsPC,
                UbiArtGameMode.RaymanAdventuresAndroid,
                UbiArtGameMode.RaymanMiniMac,
                UbiArtGameMode.ValiantHeartsAndroid,
            });
        }

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<UbiArtGameMode> GameModeSelection { get; }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Converts from the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertFromAsync()
        {
            if (GameModeSelection.SelectedValue == UbiArtGameMode.RaymanFiestaRunPC)
            {
                await ConvertFromAsync(FiestaRunLocalizationData.GetSerializer(), (data, filePath, configPath) =>
                {
                    // Save the data
                    SerializeJSON(data, filePath);
                }, new FileFilterItem("*.loc", "LOC").ToString(), new[]
                {
                    ".json"
                }, Games.RaymanFiestaRun.GetInstallDir(false));
            }
            else
            {
                var settings = GameModeSelection.SelectedValue.GetSettings();

                var fileExtension = Equals(settings.Encoding, Encoding.UTF8) ? new FileFilterItem("*.loc8", "LOC8") : new FileFilterItem("*.loc", "LOC");

                await ConvertFromAsync(UbiArtLocalizationData.GetSerializer(settings), (data, filePath, configPath) =>
                {
                    // Save the data
                    SerializeJSON(data, filePath);
                }, fileExtension.ToString(), new[]
                {
                    ".json"
                }, GameModeSelection.SelectedValue.GetGame()?.GetInstallDir(false));
            }
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            if (GameModeSelection.SelectedValue == UbiArtGameMode.RaymanFiestaRunPC)
            {
                await ConvertToAsync(FiestaRunLocalizationData.GetSerializer(), (filePath, configPath) =>
                {
                    // Read the data
                    return DeserializeJSON<FiestaRunLocalizationData>(filePath);
                }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".loc"), false);
            }
            else
            {
                var settings = GameModeSelection.SelectedValue.GetSettings();

                var fileExtension = new FileExtension(Equals(settings.Encoding, Encoding.UTF8) ? ".loc8" : ".loc");

                await ConvertToAsync(UbiArtLocalizationData.GetSerializer(settings), (filePath, configPath) =>
                {
                    // Read the data
                    return DeserializeJSON<UbiArtLocalizationData>(filePath);
                }, new FileFilterItem("*.json", "JSON").ToString(), fileExtension, false);
            }
        }

        #endregion
    }
}
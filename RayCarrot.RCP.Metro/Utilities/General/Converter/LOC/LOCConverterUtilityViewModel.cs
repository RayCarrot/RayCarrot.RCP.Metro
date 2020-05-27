using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using System.Text;
using System.Threading.Tasks;
using RayCarrot.Common;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting .loc and .loc8 files
    /// </summary>
    public class LOCConverterUtilityViewModel : BaseConverterUtilityViewModel<GameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LOCConverterUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.RaymanOriginsPC, new[]
            {
                GameMode.RaymanOriginsPC,
                GameMode.RaymanFiestaRunPC,
                GameMode.RaymanFiestaRunAndroid,
                GameMode.RaymanLegendsPC,
                GameMode.RaymanAdventuresAndroid,
                GameMode.RaymanMiniMac,
                GameMode.ValiantHeartsAndroid,
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
            var attr = GameModeSelection.SelectedValue.GetAttribute<UbiArtGameModeInfoAttribute>();
            var settings = UbiArtSettings.GetDefaultSettings(attr.Game, attr.Platform);

            if (GameModeSelection.SelectedValue == GameMode.RaymanFiestaRunPC)
            {
                await ConvertFromAsync<FiestaRunLocalizationData>(settings, (data, filePath) =>
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

                var fileExtension = Equals(settings.StringEncoding, Encoding.UTF8) ? new FileFilterItem("*.loc8", "LOC8") : new FileFilterItem("*.loc", "LOC");

                await ConvertFromAsync<UbiArtLocalizationData>(settings, (data, filePath) =>
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
            var attr = GameModeSelection.SelectedValue.GetAttribute<UbiArtGameModeInfoAttribute>();
            var settings = UbiArtSettings.GetDefaultSettings(attr.Game, attr.Platform);

            if (GameModeSelection.SelectedValue == GameMode.RaymanFiestaRunPC)
            {
                await ConvertToAsync<FiestaRunLocalizationData>(settings, (filePath, format) =>
                {
                    // Read the data
                    return DeserializeJSON<FiestaRunLocalizationData>(filePath);
                }, new FileFilterItem("*.json", "JSON").ToString(), new FileExtension(".loc"));
            }
            else
            {
                var fileExtension = new FileExtension(Equals(settings.StringEncoding, Encoding.UTF8) ? ".loc8" : ".loc");

                await ConvertToAsync<UbiArtLocalizationData>(settings, (filePath, format) =>
                {
                    // Read the data
                    return DeserializeJSON<UbiArtLocalizationData>(filePath);
                }, new FileFilterItem("*.json", "JSON").ToString(), fileExtension);
            }
        }

        #endregion
    }
}
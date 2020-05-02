using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Origins localization converter utility
    /// </summary>
    public class ROLocalizationConverterUtilityViewModel : BaseUbiArtLocalizationConverterUtilityViewModel<UbiArtLocStringValuePair[]>
    {
        /// <summary>
        /// The default localization directory for the game, if available
        /// </summary>
        protected override FileSystemPath? DefaultLocalizationDirectory => null;

        /// <summary>
        /// The localization file extension
        /// </summary>
        protected override FileExtension LocalizationFileExtension => new FileExtension(".loc");

        /// <summary>
        /// Deserializes the localization file
        /// </summary>
        /// <param name="file">The localization file</param>
        /// <returns>The data</returns>
        protected override UbiArtLocStringValuePair[] Deserialize(FileSystemPath file)
        {
            return BinarySerializableHelpers.ReadFromFile<UbiArtLocalizationData>(file, UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanOrigins, UbiArtPlatform.PC), RCFRCP.App.GetBinarySerializerLogger()).Strings;
        }

        /// <summary>
        /// Serializes the data to the localization file
        /// </summary>
        /// <param name="file">The localization file</param>
        /// <param name="data">The data</param>
        protected override void Serialize(FileSystemPath file, UbiArtLocStringValuePair[] data)
        {
            // Read the current data to get the remaining bytes
            var currentData = BinarySerializableHelpers.ReadFromFile<UbiArtLocalizationData>(file, UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanOrigins, UbiArtPlatform.PC), RCFRCP.App.GetBinarySerializerLogger());

            // Replace the string data
            currentData.Strings = data;

            // Serialize the data
            BinarySerializableHelpers.WriteToFile(currentData, file, UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanOrigins, UbiArtPlatform.PC), RCFRCP.App.GetBinarySerializerLogger());
        }
    }
}
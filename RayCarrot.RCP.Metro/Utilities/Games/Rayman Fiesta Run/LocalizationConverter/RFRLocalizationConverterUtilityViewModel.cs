using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Fiesta Run localization converter utility
    /// </summary>
    public class RFRLocalizationConverterUtilityViewModel : BaseUbiArtLocalizationConverterUtilityViewModel<UbiArtFiestaRunLocStringValuePair[]>
    {
        /// <summary>
        /// The default localization directory for the game, if available
        /// </summary>
        protected override FileSystemPath? DefaultLocalizationDirectory => Games.RaymanFiestaRun.GetInstallDir() + "resources" + "localisation";

        /// <summary>
        /// The localization file extension
        /// </summary>
        protected override FileExtension LocalizationFileExtension => new FileExtension(".loc");

        /// <summary>
        /// Deserializes the localization file
        /// </summary>
        /// <param name="file">The localization file</param>
        /// <returns>The data</returns>
        protected override UbiArtFiestaRunLocStringValuePair[] Deserialize(FileSystemPath file)
        {
            return BinarySerializableHelpers.ReadFromFile<FiestaRunLocalizationData>(file, UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanFiestaRun, UbiArtPlatform.PC), RCFRCP.App.GetBinarySerializerLogger()).Strings;
        }

        /// <summary>
        /// Serializes the data to the localization file
        /// </summary>
        /// <param name="file">The localization file</param>
        /// <param name="data">The data</param>
        protected override void Serialize(FileSystemPath file, UbiArtFiestaRunLocStringValuePair[] data)
        {
            // Read the current data to get the remaining bytes
            var currentData = BinarySerializableHelpers.ReadFromFile<FiestaRunLocalizationData>(file, UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanFiestaRun, UbiArtPlatform.PC), RCFRCP.App.GetBinarySerializerLogger());

            // Replace the string data
            currentData.Strings = data;

            // Serialize the data
            BinarySerializableHelpers.WriteToFile(currentData, file, UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanFiestaRun, UbiArtPlatform.PC), RCFRCP.App.GetBinarySerializerLogger());
        }
    }
}
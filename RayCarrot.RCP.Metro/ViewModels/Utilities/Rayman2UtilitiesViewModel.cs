using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 2 utilities
    /// </summary>
    public class Rayman2UtilitiesViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman2UtilitiesViewModel()
        {
            // Create commands
            ApplyTranslationCommand = new AsyncRelayCommand(ApplyTranslationAsync);

            // Default properties
            SelectedTranslation = Rayman2Translation.Original;
            DetailedTranslationInfo = "Original";
        }

        #endregion

        #region Private Fields

        private Rayman2Translation _selectedTranslation;

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected translation
        /// </summary>
        public Rayman2Translation SelectedTranslation
        {
            get => _selectedTranslation;
            set
            {
                _selectedTranslation = value;

                switch (value)
                {
                    case Rayman2Translation.Original:
                        DetailedTranslationInfo = "Original";
                        break;

                    case Rayman2Translation.Irish:
                        DetailedTranslationInfo = "Irish (Gaeilge) - by PluMGMK";
                        break;

                    case Rayman2Translation.Swedish:
                        DetailedTranslationInfo = "Swedish (Svenska) - by RayCarrot";
                        break;

                    case Rayman2Translation.Portuguese:
                        DetailedTranslationInfo = "Portuguese (Português) - by Haruka Tavares";
                        break;

                    case Rayman2Translation.Slovak:
                        DetailedTranslationInfo = "Slovak (Slovenský) - by MixerX";
                        break;
                }
            }
        }

        /// <summary>
        /// The detailed information for the selected translation
        /// </summary>
        public string DetailedTranslationInfo { get; set; }

        #endregion

        #region Commands

        public ICommand ApplyTranslationCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies the selected translation
        /// </summary>
        /// <returns>The task</returns>
        public async Task ApplyTranslationAsync()
        {
            try
            {
                RCF.Logger.LogInformationSource($"The Rayman 2 translation patch is downloading...");

                // Get the game install directory
                var instDir = Games.Rayman2.GetInfo().InstallDirectory;

                // Attempt to get the files
                var fixSna = GetFixSnaFilePath(instDir);
                var texturesCnt = GetTexturesCntFilePath(instDir);

                // Verify the files
                if (!fixSna.FileExists || !texturesCnt.FileExists)
                {
                    RCF.Logger.LogInformationSource($"The Rayman 2 translation patch could not be downloaded due to the required local files not being found");

                    await RCF.MessageUI.DisplayMessageAsync("The required files could not be found", "Missing files", MessageType.Error);
                    return;
                }

                // Replace the fix.sna file
                var succeeded = await App.DownloadAsync(new Uri[]
                {
                    new Uri(GetFixSnaUrl(SelectedTranslation))
                }, false, fixSna.Parent);

                if (!succeeded)
                    return;

                RCF.Logger.LogInformationSource($"The Rayman 2 fix.sna file has been downloaded");

                // Get the current textures file
                var textures = GetTexturesVersion(texturesCnt);

                RCF.Logger.LogInformationSource($"The Rayman 2 textures file has been retrieved as {textures}");

                if (textures == SelectedTranslation || 
                    (textures == Rayman2Translation.Original && SelectedTranslation == Rayman2Translation.Irish) ||
                    (textures == Rayman2Translation.Irish && SelectedTranslation == Rayman2Translation.Original))
                    return;

                var message = SelectedTranslation == Rayman2Translation.Original
                    ? "Due to the textures file having been modified by the previous translation it is recommended to replace it with the " +
                      "original version."
                    : "It is additionally recommended to also replace the game's textures file with a modified version to get the full font " +
                      "and characters required by this translation.";

                if (!await RCF.MessageUI.DisplayMessageAsync(message, "Confirm textures replacement", MessageType.Question, true))
                    return;

                RCF.Logger.LogInformationSource($"The Rayman 2 translation texture patch is downloading...");

                // Replace the textures.cnt file
                var succeeded2 = await App.DownloadAsync(new Uri[]
                {
                    new Uri(GetTexturesCntUrl(SelectedTranslation))
                }, false, texturesCnt.Parent);

                if (!succeeded2)
                    return;

                RCF.Logger.LogInformationSource($"The Rayman 2 translation has been applied");

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("All files have been successfully replaced");
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying R2 translation patch");
                await RCF.MessageUI.DisplayMessageAsync("An error occured when applying the Rayman 2 translation utility", "Error", MessageType.Error);
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the fix.sna URL for the specified Rayman 2 translation
        /// </summary>
        /// <param name="translation">The translation to get the URL for</param>
        /// <returns>The URL for the specified translation</returns>
        private static string GetFixSnaUrl(Rayman2Translation translation)
        {
            switch (translation)
            {
                case Rayman2Translation.Original:
                    return CommonUrls.R2_OriginalFixSna_URL;

                case Rayman2Translation.Irish:
                    return CommonUrls.R2_IrishFixSna_URL;

                case Rayman2Translation.Swedish:
                    return CommonUrls.R2_SwedishFixSna_URL;

                case Rayman2Translation.Portuguese:
                    return CommonUrls.R2_PortugueseFixSna_URL;

                case Rayman2Translation.Slovak:
                    return CommonUrls.R2_SlovakFixSna_URL;

                default:
                    throw new ArgumentOutOfRangeException(nameof(translation), translation, null);
            }
        }

        /// <summary>
        /// Gets the textures.cnt URL for the specified Rayman 2 translation
        /// </summary>
        /// <param name="translation">The translation to get the URL for</param>
        /// <returns>The URL for the specified translation</returns>
        private static string GetTexturesCntUrl(Rayman2Translation translation)
        {
            switch (translation)
            {
                case Rayman2Translation.Original:
                    return CommonUrls.R2_OriginalTexturesCnt_URL;

                case Rayman2Translation.Irish:
                    return CommonUrls.R2_OriginalTexturesCnt_URL;

                case Rayman2Translation.Swedish:
                    return CommonUrls.R2_SwedishTexturesCnt_URL;

                case Rayman2Translation.Portuguese:
                    return CommonUrls.R2_PortugueseTexturesCnt_URL;

                case Rayman2Translation.Slovak:
                    return CommonUrls.R2_SlovakTexturesCnt_URL;

                default:
                    throw new ArgumentOutOfRangeException(nameof(translation), translation, null);
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the file path for the textures.cnt file
        /// </summary>
        /// <param name="installDir">The game install directory</param>
        /// <returns>The file path</returns>
        public static FileSystemPath GetTexturesCntFilePath(FileSystemPath installDir)
        {
            return installDir + "Data" + "Textures.cnt";
        }

        /// <summary>
        /// Gets the file path for the fix.sna file
        /// </summary>
        /// <param name="installDir">The game install directory</param>
        /// <returns>The file path</returns>
        public static FileSystemPath GetFixSnaFilePath(FileSystemPath installDir)
        {
            return installDir + "Data" + "World" + "Levels" + "Fix.sna";
        }

        /// <summary>
        /// Gets the version of the current textures file
        /// </summary>
        /// <param name="path">The textures file path</param>
        /// <returns>The version or null if not found</returns>
        public static Rayman2Translation? GetTexturesVersion(FileSystemPath path)
        {
            if (!path.FileExists)
                return null;

            try
            {
                var size = path.GetSize();

                if (size == new ByteSize(29271236))
                    return Rayman2Translation.Original;

                if (size == new ByteSize(29276103))
                    return Rayman2Translation.Swedish;

                if (size == new ByteSize(29274254))
                    return Rayman2Translation.Portuguese;

                if (size == new ByteSize(29116088))
                    return Rayman2Translation.Slovak;

                return null;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting R2 textures file size");
                return null;
            }
        }

        #endregion

        #region Public Enums

        /// <summary>
        /// The available Rayman 2 translation
        /// </summary>
        public enum Rayman2Translation
        {
            Original,
            Irish,
            Swedish,
            Portuguese,
            Slovak
        }

        #endregion
    }
}
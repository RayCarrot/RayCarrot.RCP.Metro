using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 2 translation utility
    /// </summary>
    public class R2TranslationUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R2TranslationUtilityViewModel()
        {
            // Create commands
            ApplyTranslationCommand = new AsyncRelayCommand(ApplyTranslationAsync);

            // Get the game info
            GameInfo = Games.Rayman2.GetInfo();

            // Get current translation
            SelectedTranslation = GetAppliedRayman2Translation() ?? Rayman2Translation.Original;

            RCFCore.Logger?.LogInformationSource($"The applied Rayman 2 translation has been detected as {SelectedTranslation}");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected translation
        /// </summary>
        public Rayman2Translation SelectedTranslation { get; set; }

        /// <summary>
        /// The game info
        /// </summary>
        public GameInfo GameInfo { get; }

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
                RCFCore.Logger?.LogInformationSource($"The Rayman 2 translation patch is downloading...");

                // Attempt to get the files
                var fixSna = GetFixSnaFilePath();
                var texturesCnt = GetTexturesCntFilePath();

                // Verify the files
                if (!fixSna.FileExists || !texturesCnt.FileExists)
                {
                    RCFCore.Logger?.LogInformationSource($"The Rayman 2 translation patch could not be downloaded due to the required local files not being found");

                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.R2U_Translations_FilesNotFound, MessageType.Error);
                    return;
                }

                // Replace the fix.sna file
                var succeeded = await App.DownloadAsync(new Uri[]
                {
                    new Uri(GetFixSnaUrl(SelectedTranslation))
                }, false, fixSna.Parent);

                if (!succeeded)
                    return;

                RCFCore.Logger?.LogInformationSource($"The Rayman 2 fix.sna file has been downloaded");

                // Get the current textures file
                var textures = GetTexturesVersion(texturesCnt);

                RCFCore.Logger?.LogInformationSource($"The Rayman 2 textures file has been retrieved as {textures}");

                if (textures == SelectedTranslation || 
                    (textures == Rayman2Translation.Original && SelectedTranslation == Rayman2Translation.Irish) ||
                    (textures == Rayman2Translation.Irish && SelectedTranslation == Rayman2Translation.Original))
                    return;

                var message = SelectedTranslation == Rayman2Translation.Original
                    ? Resources.R2U_Translations_RevertTextures
                    : Resources.R2U_Translations_ReplaceTextures;

                if (await RCFUI.MessageUI.DisplayMessageAsync(message, Resources.R2U_Translations_ReplaceTexturesHeader, MessageType.Question, true))
                {
                    RCFCore.Logger?.LogInformationSource($"The Rayman 2 translation texture patch is downloading...");

                    // Replace the textures.cnt file
                    var succeeded2 = await App.DownloadAsync(new Uri[]
                    {
                        new Uri(GetTexturesCntUrl(SelectedTranslation))
                    }, false, texturesCnt.Parent);

                    if (!succeeded2)
                        return;
                }

                RCFCore.Logger?.LogInformationSource($"The Rayman 2 translation has been applied");

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.R2U_Translations_Success);
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying R2 translation patch");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R2U_Translations_Error);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the fix.sna URL for the specified Rayman 2 translation
        /// </summary>
        /// <param name="translation">The translation to get the URL for</param>
        /// <returns>The URL for the specified translation</returns>
        public string GetFixSnaUrl(Rayman2Translation translation)
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
        public string GetTexturesCntUrl(Rayman2Translation translation)
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

        /// <summary>
        /// Gets the file path for the textures.cnt file
        /// </summary>
        /// <returns>The file path</returns>
        public FileSystemPath GetTexturesCntFilePath()
        {
            return GameInfo.InstallDirectory + "Data" + "Textures.cnt";
        }

        /// <summary>
        /// Gets the file path for the fix.sna file
        /// </summary>
        /// <returns>The file path</returns>
        public FileSystemPath GetFixSnaFilePath()
        {
            return GameInfo.InstallDirectory + "Data" + "World" + "Levels" + "Fix.sna";
        }

        /// <summary>
        /// Gets the version of the current textures file
        /// </summary>
        /// <param name="path">The textures file path</param>
        /// <returns>The version or null if not found</returns>
        public Rayman2Translation? GetTexturesVersion(FileSystemPath path)
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

        /// <summary>
        /// Gets the currently applied Rayman 2 translation
        /// </summary>
        /// <returns>The applied translation or null in case of error or unknown version</returns>
        public Rayman2Translation? GetAppliedRayman2Translation()
        {
            try
            {
                // Get the checksum
                var hash = GetFixSnaFilePath().GetSHA256CheckSum();

                switch (hash)
                {
                    case "0A1A9A86D20CB69F978E7C897A511CDB77C2C948149669E5A34EC1C74DA21147":
                        return Rayman2Translation.Original;

                    case "93F75AE59EAB1FA16DA21FCD88D811CA28220CCFC8826565B72FA571BAFADB83":
                        return Rayman2Translation.Swedish;

                    case "3DEAABD8E0956C63AF5E17A34CD70CDA4FAC528D78EFCA69E0039D85E4312CC5":
                        return Rayman2Translation.Slovak;

                    case "ED22B3087C52DAECE6DB653E47AAF84F3E0EAAF3C59ED2BBF9C91354CA9669AA":
                        return Rayman2Translation.Irish;

                    case "14D84894F5B14891F42FE1F2A31109A9064435B78907BB0DA90B086DFAB029FE":
                        return Rayman2Translation.Portuguese;

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting applied R2 translation");
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
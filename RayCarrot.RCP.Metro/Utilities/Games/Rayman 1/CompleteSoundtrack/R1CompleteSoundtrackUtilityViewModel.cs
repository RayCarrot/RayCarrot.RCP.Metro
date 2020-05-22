using ByteSizeLib;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 1 complete soundtrack utility
    /// </summary>
    public class R1CompleteSoundtrackUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1CompleteSoundtrackUtilityViewModel()
        {
            // Create the commands
            ReplaceSoundtrackCommand = new AsyncRelayCommand(ReplaceSoundtrackAsync);

            // Attempt to find the Rayman Forever music directory
            var dir = Games.Rayman1.GetInstallDir(false).Parent + "Music";

            // Set to music path if found
            MusicDir = dir.DirectoryExists && (dir + "rayman02.ogg").FileExists ? dir : FileSystemPath.EmptyPath;

            // Indicate if music can be replaces
            CanMusicBeReplaced = MusicDir.DirectoryExists;

            if (CanMusicBeReplaced)
                IsOriginalMusic = GetIsOriginalSoundtrack() ?? false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The Rayman Forever music directory
        /// </summary>
        public FileSystemPath MusicDir { get; }

        /// <summary>
        /// Indicates if the Rayman Forever music can be replaced
        /// </summary>
        public bool CanMusicBeReplaced { get; set; }

        /// <summary>
        /// Indicates if the current music files are the original ones
        /// </summary>
        public bool IsOriginalMusic { get; set; }

        #endregion

        #region Commands

        public ICommand ReplaceSoundtrackCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Replaces the current soundtrack
        /// </summary>
        /// <returns>The task</returns>
        public async Task ReplaceSoundtrackAsync()
        {
            try
            {
                RL.Logger?.LogInformationSource($"The Rayman 1 soundtrack is being replaced with the {(IsOriginalMusic ? "complete version" : "original version")}");

                // Download the files
                var succeeded = await App.DownloadAsync(new Uri[]
                {
                    new Uri(IsOriginalMusic ? CommonUrls.R1_CompleteOST_URL : CommonUrls.R1_IncompleteOST_URL)
                }, true, MusicDir);

                if (succeeded)
                    IsOriginalMusic ^= true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Replacing R1 soundtrack");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_CompleteOSTReplaceError);
            }
        }

        /// <summary>
        /// Gets a value indicating if the original soundtrack is available in the specified path
        /// </summary>
        /// <returns>True if the original soundtrack is available, false if not. Null if an error occurred while checking.</returns>
        public bool? GetIsOriginalSoundtrack()
        {
            try
            {
                var file = MusicDir + "rayman02.ogg";

                if (!file.FileExists)
                    return null;

                var size = file.GetSize();

                return size == ByteSize.FromBytes(1805221);
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting R1 music size");
                return null;
            }
        }

        #endregion
    }
}
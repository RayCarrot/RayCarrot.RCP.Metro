using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// VIew model Rayman 1 utilities
    /// </summary>
    public class Rayman1UtilitiesViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman1UtilitiesViewModel()
        {
            // Create the commands
            ReplaceSoundtrackCommand = new AsyncRelayCommand(ReplaceSoundtrackAsync);

            // Attempt to find the Rayman Forever music directory
            var dir = GetMusicDirectory();

            // Set to music path if found
            MusicDir = dir.DirectoryExists && (dir + "rayman02.ogg").FileExists ? dir : FileSystemPath.EmptyPath;

            // Indicate if music can be replaces
            CanMusicBeReplaced = MusicDir.DirectoryExists;

            if (CanMusicBeReplaced)
                IsOriginalMusic = GetIsOriginalSoundtrack(MusicDir) ?? false;
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
                RCFCore.Logger?.LogInformationSource($"The Rayman 1 soundtrack is being replaced with the {(IsOriginalMusic ? "complete version" : "original version")}");

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
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.R1U_CompleteOSTReplaceError, MessageType.Error);
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets a value indicating if the original soundtrack is available in the specified path
        /// </summary>
        /// <param name="path">The music directory</param>
        /// <returns>True if the original soundtrack is available, false if not. Null if an error occurred while checking.</returns>
        public static bool? GetIsOriginalSoundtrack(FileSystemPath path)
        {
            try
            {
                var file = path + "rayman02.ogg";

                if (!file.FileExists)
                    return null;

                var size = file.GetSize();

                return size == new ByteSize(1805221);
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting R1 music size");
                return null;
            }
        }

        /// <summary>
        /// Gets the game music directory
        /// </summary>
        /// <returns>The music director path</returns>
        public static FileSystemPath GetMusicDirectory()
        {
            return Games.Rayman1.GetInfo().InstallDirectory.Parent + "Music";
        }

        #endregion
    }
}
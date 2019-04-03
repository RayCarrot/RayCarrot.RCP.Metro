using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using RayCarrot.CarrotFramework;

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
            InstallTPLSCommand = new AsyncRelayCommand(InstallTPLSAsync);
            UninstallTPLSCommand = new AsyncRelayCommand(UninstallTPLSAsync);
            ReplaceSoundtrackCommand = new AsyncRelayCommand(ReplaceSoundtrackAsync);

            // Check if TPLS is installed under the default location
            if (CommonPaths.TPLSDir.DirectoryExists)
            {
                if (Data.TPLSData == null)
                    Data.TPLSData = new TPLSData(CommonPaths.TPLSDir);
            }
            else
            {
                Data.TPLSData = null;
            }

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

        public ICommand InstallTPLSCommand { get; }

        public ICommand UninstallTPLSCommand { get; }

        public ICommand ReplaceSoundtrackCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Installs TPLS
        /// </summary>
        public async Task InstallTPLSAsync()
        {
            // Verify the install directory
            if (!await VerifyInstallDirAsync(Games.Rayman1.GetInfo().InstallDirectory))
                return;

            try
            {
                RCF.Logger.LogInformationSource($"The TPLS utility is downloading...");

                // Check if the directory exists
                if (CommonPaths.TPLSDir.DirectoryExists)
                    // Delete the directory
                    RCFRCP.File.DeleteDirectory(CommonPaths.TPLSDir);

                // Download the files
                if (!await App.DownloadAsync(new Uri[]
                {
                    new Uri(CommonUrls.R1_TPLS_Url), 
                }, true, CommonPaths.TPLSDir))
                {
                    // If cancelled, delete the directory
                    RCFRCP.File.DeleteDirectory(CommonPaths.TPLSDir);
                    return;
                }

                // Save
                RCFRCP.Data.TPLSData = new TPLSData(CommonPaths.TPLSDir);

                RCF.Logger.LogInformationSource($"The TPLS utility has been downloaded");
            }
            catch (Exception ex)
            {
                ex.HandleError("Installing TPLS");
                await RCF.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSInstallationFailed, Resources.R1U_TPLSInstallationFailedHeader, MessageType.Error);
            }
        }

        /// <summary>
        /// Uninstalls TPLS
        /// </summary>
        public async Task UninstallTPLSAsync()
        {
            // Have user confirm uninstall
            if (!await RCF.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSConfirmUninstall, Resources.R1U_TPLSConfirmUninstallHeader, MessageType.Question, true))
                return;

            try
            {
                RCFRCP.File.DeleteDirectory(RCFRCP.Data.TPLSData.InstallDir);
                await RCF.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSUninstallSuccess, Resources.R1U_TPLSUninstallSuccessHeader, MessageType.Success);

                RCFRCP.Data.TPLSData = null;

                RCF.Logger.LogInformationSource($"The TPLS utility has been uninstalled");
            }
            catch (Exception ex)
            {
                ex.HandleError("Uninstalling TPLS");
                await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.R1U_TPLSUninstallError, ex.Message), Resources.R1U_TPLSUninstallErrorHeader, MessageType.Error);
            }
        }

        /// <summary>
        /// Replaces the current soundtrack
        /// </summary>
        /// <returns>The task</returns>
        public async Task ReplaceSoundtrackAsync()
        {
            try
            {
                RCF.Logger.LogInformationSource($"The Rayman 1 soundtrack is being replaced with the {(IsOriginalMusic ? "complete version" : "original version")}");

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
                await RCF.MessageUI.DisplayMessageAsync(Resources.R1U_CompleteOSTReplaceError, MessageType.Error);
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Verifies the specified install directory for a valid Rayman installation
        /// </summary>
        /// <param name="dir">The directory</param>
        /// <returns>True if it is valid, false if not</returns>
        private static async Task<bool> VerifyInstallDirAsync(FileSystemPath dir)
        {
            var files = new FileSystemPath[]
            {
                dir + "RAYMAN.EXE",
                dir + "VIGNET.DAT",
            };

            if (!files.FilesExist() || !Directory.Exists(dir + "PCMAP"))
            {
                await RCF.MessageUI.DisplayMessageAsync(Resources.R1U_TPLSInvalidDirectory, Resources.R1U_TPLSInvalidDirectoryHeader, MessageType.Error);
                return false;
            }

            return true;
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
                var size = (path + "rayman02.ogg").GetSize();

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
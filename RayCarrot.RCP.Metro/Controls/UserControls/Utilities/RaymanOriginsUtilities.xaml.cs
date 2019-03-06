using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using Microsoft.WindowsAPICodePack.Shell;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for RaymanOriginsUtilities.xaml
    /// </summary>
    public partial class RaymanOriginsUtilities : BaseUserControl<RaymanOriginsUtilitiesViewModel>
    {
        public RaymanOriginsUtilities()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// View model for the Rayman Origins utilities
    /// </summary>
    public class RaymanOriginsUtilitiesViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanOriginsUtilitiesViewModel()
        {
            // Create the commands
            ReplaceVideosCommand = new AsyncRelayCommand(ReplaceVideosAsync);
            UpdateDiscVersionCommand = new AsyncRelayCommand(UpdateDiscVersionAsync);

            // Attempt to find the Rayman Origins video directory
            var dir = Games.RaymanOrigins.GetInfo().InstallDirectory + "GameData";

            // Set to music path if found
            VideoDir = dir.DirectoryExists && (dir + "intro.bik").FileExists ? dir : FileSystemPath.EmptyPath;

            // Indicate if music can be replaces
            CanVideosBeReplaced = VideoDir.DirectoryExists;

            if (CanVideosBeReplaced)
            {
                try
                {
                    var size = (dir + "intro.bik").GetSize();

                    IsOriginalVideos = size == new ByteSize(59748732);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Getting RO video size");
                    CanVideosBeReplaced = false;
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The Rayman Origins video directory
        /// </summary>
        public FileSystemPath VideoDir { get; }

        /// <summary>
        /// Indicates if the Rayman Origins videos can be replaced
        /// </summary>
        public bool CanVideosBeReplaced { get; set; }

        /// <summary>
        /// Indicates if the current video files are the original ones
        /// </summary>
        public bool IsOriginalVideos { get; set; }

        #endregion

        #region Commands

        public ICommand ReplaceVideosCommand { get; }

        public ICommand UpdateDiscVersionCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Replaces the current videos
        /// </summary>
        /// <returns>The task</returns>
        public async Task ReplaceVideosAsync()
        {
            try
            {
                // Download the files
                var succeeded = await App.DownloadAsync(new Uri[]
                {
                    new Uri(IsOriginalVideos ? CommonUrls.RO_HQVideos_URL : CommonUrls.RO_OriginalVideos_URL), 
                }, true, VideoDir);

                if (succeeded)
                    IsOriginalVideos ^= true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Replacing RO videos");
                await RCF.MessageUI.DisplayMessageAsync("Video replacement failed.", "Error", MessageType.Error);
            }
        }

        /// <summary>
        /// Updates the disc version to the latest version (1.02)
        /// </summary>
        /// <returns>The task</returns>
        public async Task UpdateDiscVersionAsync()
        {
            try
            {
                // Download the file
                var succeeded = await App.DownloadAsync(new Uri[]
                {
                    new Uri(CommonUrls.RO_Updater_URL)
                }, true, KnownFolders.Downloads.Path);

                if (succeeded)
                    (await RCFRCP.File.LaunchFileAsync(Path.Combine(KnownFolders.Downloads.Path, "RaymanOriginspc_1.02.exe")))?.Dispose();
            }
            catch (Exception ex)
            {
                ex.HandleError("Downloading RO updater");
                await RCF.MessageUI.DisplayMessageAsync("Downloading the Rayman Origins updater failed.", "Error", MessageType.Error);
            }


        }

        #endregion
    }
}
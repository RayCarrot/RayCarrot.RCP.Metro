using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Origins HQ Videos utility
/// </summary>
public class Utility_RaymanOrigins_HQVideos_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanOrigins_HQVideos_ViewModel()
    {
        // Create the commands
        ReplaceVideosCommand = new AsyncRelayCommand(ReplaceVideosAsync);

        // Attempt to find the Rayman Origins video directory
        var dir = Games.RaymanOrigins.GetInstallDir(false) + "GameData";

        // Set to music path if found
        VideoDir = dir.DirectoryExists && (dir + "intro.bik").FileExists ? dir : FileSystemPath.EmptyPath;

        // Indicate if music can be replaces
        CanVideosBeReplaced = VideoDir.DirectoryExists;

        if (!CanVideosBeReplaced)
            return;

        var result = GetIsOriginalVideos();

        if (result == null)
            CanVideosBeReplaced = false;
        else
            IsOriginalVideos = result.Value;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Info("The Rayman Origins videos are being replaced with {0}", IsOriginalVideos ? "HQ Videos" : "original videos");

            // Download the files
            var succeeded = await App.DownloadAsync(new Uri[]
            {
                new Uri(IsOriginalVideos ? AppURLs.RO_HQVideos_URL : AppURLs.RO_OriginalVideos_URL),
            }, true, VideoDir);

            if (succeeded)
                IsOriginalVideos ^= true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Replacing RO videos");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.ROU_HQVideosFailed);
        }
    }

    /// <summary>
    /// Gets a value indicating if the original videos are available in the specified path
    /// </summary>
    /// <returns>True if the original videos are available, false if not. Null if an error occurred while checking.</returns>
    public bool? GetIsOriginalVideos()
    {
        try
        {
            var file = VideoDir + "intro.bik";

            if (!file.FileExists)
                return null;

            var size = file.GetSize();

            return size == ByteSize.FromBytes(59748732);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting RO video size");
            return null;
        }
    }

    #endregion
}
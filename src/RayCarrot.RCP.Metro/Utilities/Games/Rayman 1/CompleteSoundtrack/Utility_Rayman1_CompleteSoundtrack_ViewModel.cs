﻿#nullable disable
using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman 1 complete soundtrack utility
/// </summary>
public class Utility_Rayman1_CompleteSoundtrack_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Rayman1_CompleteSoundtrack_ViewModel(GameInstallation gameInstallation)
    {
        // Create the commands
        ReplaceSoundtrackCommand = new AsyncRelayCommand(ReplaceSoundtrackAsync);

        // Attempt to find the Rayman Forever music directory
        var dir = gameInstallation.InstallLocation.Directory.Parent + "Music";

        // Set to music path if found
        MusicDir = dir.DirectoryExists && (dir + "rayman02.ogg").FileExists ? dir : FileSystemPath.EmptyPath;

        // Indicate if music can be replaces
        CanMusicBeReplaced = MusicDir.DirectoryExists;

        if (CanMusicBeReplaced)
            IsOriginalMusic = GetIsOriginalSoundtrack() ?? false;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Info("The Rayman 1 soundtrack is being replaced with the {0}", IsOriginalMusic ? "complete version" : "original version");

            // Download the files
            var succeeded = await App.DownloadAsync(new Uri[]
            {
                new Uri(IsOriginalMusic ? AppURLs.R1_CompleteOST_URL : AppURLs.R1_IncompleteOST_URL)
            }, true, MusicDir);

            if (succeeded)
                IsOriginalMusic ^= true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Replacing R1 soundtrack");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.R1U_CompleteOSTReplaceError);
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

            long size = file.GetSize();

            return size == 1805221;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting R1 music size");
            return null;
        }
    }

    #endregion
}
#nullable disable
using System.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the DOSBox games options
/// </summary>
public class GameOptions_DOSBox_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation</param>
    public GameOptions_DOSBox_ViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The game installation
    /// </summary>
    public GameInstallation GameInstallation { get; }

    /// <summary>
    /// The allowed drive types when browsing for a mount path
    /// </summary>
    public DriveType[] MountPathAllowedDriveTypes => new DriveType[]
    {
        DriveType.CDRom
    };

    /// <summary>
    /// The file or directory to mount
    /// </summary>
    public FileSystemPath MountPath
    {
        get => Data.Game_DosBoxGames[GameInstallation.Game].MountPath;
        set
        {
            Data.Game_DosBoxGames[GameInstallation.Game].MountPath = value;

            // TODO: Find better solution to this. Ideally we would invoke the refresh from an event caused by the UI, but
            // currently the BrowseBox does not have any event for when the path is changed. Doing this rather than discarding the task
            // from the async refresh will ensure that any exceptions are handled correctly as the async void will take care of that.
            Invoke();
            async void Invoke() => await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(GameInstallation, RefreshFlags.GameInfo));
        }
    }

    #endregion
}
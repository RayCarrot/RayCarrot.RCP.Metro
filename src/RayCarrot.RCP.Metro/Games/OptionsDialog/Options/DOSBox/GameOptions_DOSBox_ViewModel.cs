using System.IO;
using CommunityToolkit.Mvvm.Messaging;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the DOSBox games options
/// </summary>
public class GameOptions_DOSBox_ViewModel : BaseViewModel
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
    public DriveType[] MountPathAllowedDriveTypes => new[] { DriveType.CDRom };

    /// <summary>
    /// The file or directory to mount
    /// </summary>
    public FileSystemPath MountPath
    {
        get => GameInstallation.GetValue<FileSystemPath>(GameDataKey.DOSBoxMountPath);
        set
        {
            GameInstallation.SetValue(GameDataKey.DOSBoxMountPath, value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }

    #endregion
}
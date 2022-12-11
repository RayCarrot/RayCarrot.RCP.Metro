using System.IO;

namespace RayCarrot.RCP.Metro;

// TODO-14: Rework

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
        get => GameInstallation.GetValue<FileSystemPath>(GameDataKey.Emu_DosBox_MountPath);
        set
        {
            GameInstallation.SetValue(GameDataKey.Emu_DosBox_MountPath, value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }

    #endregion
}
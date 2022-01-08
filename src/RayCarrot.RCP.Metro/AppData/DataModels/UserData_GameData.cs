namespace RayCarrot.RCP.Metro;

/// <summary>
/// Game data for a game
/// </summary>
public class UserData_GameData
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameType">The game type</param>
    /// <param name="installDirectory">The install directory</param>
    public UserData_GameData(GameType gameType, FileSystemPath installDirectory)
    {
        GameType = gameType;
        InstallDirectory = installDirectory;
        LaunchMode = UserData_GameLaunchMode.AsInvoker;
    }

    /// <summary>
    /// The game type
    /// </summary>
    public GameType GameType { get; }

    /// <summary>
    /// The install directory
    /// </summary>
    public FileSystemPath InstallDirectory { get; }

    /// <summary>
    /// The game launch mode
    /// </summary>
    public UserData_GameLaunchMode LaunchMode { get; set; }
}
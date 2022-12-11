namespace RayCarrot.RCP.Metro;

/// <summary>
/// A finder item base
/// </summary>
public abstract class GameFinder_BaseItem
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="possibleWin32Names">The possible names of the game to search for. This is not case sensitive, but most match entire string.</param>
    /// <param name="shortcutName">The shortcut name when searching shortcuts</param>
    /// <param name="verifyInstallDirectory">Optional method for verifying the found install directory</param>
    /// <param name="customFinderAction">Custom game finder action which returns the game install directory if found</param>
    protected GameFinder_BaseItem(
        string[]? possibleWin32Names, 
        string? shortcutName, 
        Func<FileSystemPath, FileSystemPath?>? verifyInstallDirectory, 
        Func<GameFinder_FoundResult?>? customFinderAction)
    {
        PossibleWin32Names = possibleWin32Names;
        ShortcutName = shortcutName;
        VerifyInstallDirectory = verifyInstallDirectory;
        CustomFinderAction = customFinderAction;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The possible names of the game to search for. This is not case sensitive, but most match entire string.
    /// </summary>
    public string[]? PossibleWin32Names { get; }

    /// <summary>
    /// The shortcut name when searching shortcuts
    /// </summary>
    public string? ShortcutName { get; }

    /// <summary>
    /// Optional method for verifying the found install directory
    /// </summary>
    public Func<FileSystemPath, FileSystemPath?>? VerifyInstallDirectory { get; }

    /// <summary>
    /// Custom game finder action which returns the game install directory if found
    /// </summary>
    public Func<GameFinder_FoundResult?>? CustomFinderAction { get; }

    #endregion
}
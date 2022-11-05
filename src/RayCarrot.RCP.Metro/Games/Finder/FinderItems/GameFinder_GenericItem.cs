using System;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A finder item
/// </summary>
public class GameFinder_GenericItem : GameFinder_BaseItem
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="possibleWin32Names">The possible names of the game to search for. This is not case sensitive, but most match entire string.</param>
    /// <param name="shortcutName">The shortcut name when searching shortcuts</param>
    /// <param name="verifyInstallDirectory">Optional method for verifying the found install directory</param>
    /// <param name="foundAction">The action to run once the item has been found</param>
    /// <param name="displayName">The item display name</param>
    /// <param name="customFinderAction">Custom game finder action which returns the game install directory if found</param>
    public GameFinder_GenericItem(
        string[]? possibleWin32Names, 
        string? shortcutName, 
        Func<FileSystemPath, FileSystemPath?>? verifyInstallDirectory, 
        Action<FileSystemPath, object?>? foundAction, 
        string displayName, 
        Func<GameFinder_FoundResult?>? customFinderAction = null) 
        : base(possibleWin32Names, shortcutName, verifyInstallDirectory, foundAction, customFinderAction)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// The item display name
    /// </summary>
    public string DisplayName { get; }
}
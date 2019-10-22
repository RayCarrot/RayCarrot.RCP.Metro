using System;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A finder item
    /// </summary>
    public class FinderItem : BaseFinderItem
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="possibleWin32Names">The possible names of the game to search for. This is not case sensitive, but most match entire string.</param>
        /// <param name="shortcutName">The shortcut name when searching shortcuts</param>
        /// <param name="verifyInstallDirectory">Optional method for verifying the found install directory</param>
        /// <param name="foundAction">The action to run once the item has been found</param>
        public FinderItem(string[] possibleWin32Names, string shortcutName, Func<FileSystemPath, FileSystemPath?> verifyInstallDirectory, Action<FileSystemPath> foundAction) : base(possibleWin32Names, shortcutName, verifyInstallDirectory)
        {
            FoundAction = foundAction;
        }

        /// <summary>
        /// The action to run once the item has been found
        /// </summary>
        public Action<FileSystemPath> FoundAction { get; }
    }
}
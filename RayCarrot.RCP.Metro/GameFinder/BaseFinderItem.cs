using System;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A finder item base
    /// </summary>
    public abstract class BaseFinderItem
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="possibleWin32Names">The possible names of the game to search for. This is not case sensitive, but most match entire string.</param>
        /// <param name="shortcutName">The shortcut name when searching shortcuts</param>
        /// <param name="verifyInstallDirectory">Optional method for verifying the found install directory</param>
        protected BaseFinderItem(string[] possibleWin32Names, string shortcutName, Func<FileSystemPath, FileSystemPath?> verifyInstallDirectory)
        {
            PossibleWin32Names = possibleWin32Names;
            ShortcutName = shortcutName;
            VerifyInstallDirectory = verifyInstallDirectory;
        }

        /// <summary>
        /// The possible names of the game to search for. This is not case sensitive, but most match entire string.
        /// </summary>
        public string[] PossibleWin32Names { get; }

        /// <summary>
        /// The shortcut name when searching shortcuts
        /// </summary>
        public string ShortcutName { get; }

        /// <summary>
        /// Optional method for verifying the found install directory
        /// </summary>
        public Func<FileSystemPath, FileSystemPath?> VerifyInstallDirectory { get; }
    }
}
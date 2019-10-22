﻿using System;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A game finder item
    /// </summary>
    public class GameFinderItem : BaseFinderItem
    {
        #region Constructors

        /// <summary>
        /// Constructor for a game finder item with a ubi.ini section name, shortcut name and possible Win32 names
        /// </summary>
        /// <param name="ubiIniSectionName">The section name for the ubi.ini file, if it is to be searched</param>
        /// <param name="shortcutName">The shortcut name when searching shortcuts</param>
        /// <param name="possibleWin32Names">The possible names of the game to search for. This is not case sensitive, but most match entire string.</param>
        /// <param name="verifyInstallDirectory">Optional method for verifying the found install directory</param>
        public GameFinderItem(string ubiIniSectionName, string shortcutName, string[] possibleWin32Names, Func<FileSystemPath, FileSystemPath?> verifyInstallDirectory = null) : base(possibleWin32Names, shortcutName, verifyInstallDirectory)
        {
            UbiIniSectionName = ubiIniSectionName;
        }

        /// <summary>
        /// Constructor for a game finder item with a Steam ID
        /// </summary>
        /// <param name="steamID">The Steam ID to search for</param>
        /// <param name="verifyInstallDirectory">Optional method for verifying the found install directory</param>
        public GameFinderItem(string steamID, Func<FileSystemPath, FileSystemPath?> verifyInstallDirectory = null) : base(null, null, verifyInstallDirectory)
        {
            SteamID = steamID;
        }

        /// <summary>
        /// Constructor for a game finder with a custom finder action
        /// </summary>
        /// <param name="customFinderAction">Custom game finder action which return the game install directory if found</param>
        public GameFinderItem(Func<FileSystemPath?> customFinderAction) : base(null, null, null)
        {
            CustomFinderAction = customFinderAction;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The section name for the ubi.ini file, if it is to be searched
        /// </summary>
        public string UbiIniSectionName { get; }

        /// <summary>
        /// The Steam ID to search for
        /// </summary>
        public string SteamID { get; }

        /// <summary>
        /// Custom game finder action which return the game install directory if found
        /// </summary>
        public Func<FileSystemPath?> CustomFinderAction { get; }

        #endregion
    }
}
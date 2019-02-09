using System;
using System.Collections.Generic;
using System.Windows;
using RayCarrot.CarrotFramework;
using RayCarrot.UserData;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The application user data
    /// </summary>
    public class AppUserData : BaseViewModel, IUserData
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AppUserData()
        {
            // Reset the properties
            Reset();
        }

        #endregion

        #region Interface Implementation

        /// <summary>
        /// The path of the saved <see cref="AppUserData"/> file
        /// </summary>
        public FileSystemPath FilePath => CommonPaths.AppUserDataPath;

        /// <summary>
        /// The name of the <see cref="IUserData"/>
        /// </summary>
        public string Name => nameof(AppUserData);

        /// <summary>
        /// Resets all values to their defaults
        /// </summary>
        public void Reset()
        {
            Games = new Dictionary<Games, GameInfo>();
            DosBoxGames = new Dictionary<Games, DosBoxOptions>();
            UserLevel = UserLevel.Normal;
            IsFirstLaunch = true;
            LastVersion = RCFRCP.App.CurrentVersion;
            WindowState = null;
            DarkMode = true;
            ShowActionComplete = true;
            DosBoxPath = FileSystemPath.EmptyPath;
            DosBoxConfig = FileSystemPath.EmptyPath;
            DialogAsWindow = true;
            AutoLocateGames = true;
        }

        #endregion

        #region Private Fields

        private bool _darkMode;

        #endregion

        #region Public Properties

        /// <summary>
        /// The saved games
        /// </summary>
        public Dictionary<Games, GameInfo> Games { get; set; }

        /// <summary>
        /// The DosBox games options
        /// </summary>
        public Dictionary<Games, DosBoxOptions> DosBoxGames { get; set; }

        /// <summary>
        /// The current user level
        /// </summary>
        public UserLevel UserLevel
        {
            get => RCF.Data.CurrentUserLevel;
            set => RCF.Data.CurrentUserLevel = value;
        }

        /// <summary>
        /// Indicates if it is the first time the application launches
        /// </summary>
        public bool IsFirstLaunch { get; set; }

        /// <summary>
        /// The last used version of the program
        /// </summary>
        public Version LastVersion { get; set; }

        /// <summary>
        /// The previous Window session state
        /// </summary>
        public WindowSessionState WindowState { get; set; }

        /// <summary>
        /// Indicates if the dark mode is on
        /// </summary>
        public bool DarkMode
        {
            get => _darkMode;
            set
            {
                _darkMode = value;
                MahApps.Metro.ThemeManager.ChangeAppTheme(Application.Current, $"Base{(DarkMode ? "Dark" : "Light")}");
            }
        }

        /// <summary>
        /// Indicates if action complete messages should be shown
        /// </summary>
        public bool ShowActionComplete { get; set; }

        /// <summary>
        /// The path for the DosBox file
        /// </summary>
        public string DosBoxPath { get; set; }

        /// <summary>
        /// Optional path for the DosBox config file
        /// </summary>
        public string DosBoxConfig { get; set; }

        /// <summary>
        /// True if all dialogs should be shown as windows
        /// or false if the primary dialogs should be shown
        /// as metro dialogs
        /// </summary>
        public bool DialogAsWindow { get; set; }

        /// <summary>
        /// Indicates if games should be automatically located on startup
        /// </summary>
        public bool AutoLocateGames { get; set; }

        #endregion
    }

    /// <summary>
    /// Options for a DosBox game
    /// </summary>
    public class DosBoxOptions
    {
        /// <summary>
        /// The file or directory to mount
        /// </summary>
        public FileSystemPath MountPath { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Infralution.Localization.Wpf;
using MahApps.Metro;
using Newtonsoft.Json;
using RayCarrot.CarrotFramework;
using RayCarrot.UserData;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The application user data
    /// </summary>
    public class AppUserData : BaseViewModel, IUserData
    {
        #region Interface Implementation

        /// <summary>
        /// The path of the saved <see cref="AppUserData"/> file
        /// </summary>
        [JsonIgnore]
        public FileSystemPath FilePath => CommonPaths.AppUserDataPath;

        /// <summary>
        /// The name of the <see cref="IUserData"/>
        /// </summary>
        [JsonIgnore]
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
            LastVersion = new Version(0, 0, 0, 0);
            WindowState = null;
            DarkMode = true;
            ShowActionComplete = true;
            DosBoxPath = FileSystemPath.EmptyPath;
            DosBoxConfig = FileSystemPath.EmptyPath;
            AutoLocateGames = true;
            AutoUpdate = true;
            IsFiestaRunWin10Edition = true;
            ShowNotInstalledGames = true;
            CloseAppOnGameLaunch = false;
            CloseConfigOnSave = true;
            BackupLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ShowProgressOnTaskBar = true;
            DisplayExceptionLevel = ExceptionLevel.Critical;
            RRRIsSaveDataInInstallDir = true;
            ShowDetailedGameInfo = false;
            TPLSData = null;
            EnableAnimations = true;
            CurrentCulture = AppLanguages.DefaultCulture.Name;
            ShowIncompleteTranslations = false;
        }

        #endregion

        #region Private Fields

        private bool _darkMode;

        private FileSystemPath _backupLocation;

        private bool _showIncompleteTranslations;

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
        public Version LastVersion
        {
            get;
            set;
        }

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
                ThemeManager.ChangeAppTheme(Application.Current, $"Base{(DarkMode ? "Dark" : "Light")}");
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
        /// Indicates if games should be automatically located on startup
        /// </summary>
        public bool AutoLocateGames { get; set; }

        /// <summary>
        /// Indicates if updates should automatically be check for
        /// </summary>
        public bool AutoUpdate { get; set; }

        /// <summary>
        /// Indicates if the version of Rayman Fiesta Run is the Windows 10 Edition
        /// </summary>
        public bool IsFiestaRunWin10Edition { get; set; }

        /// <summary>
        /// Indicates if not installed games should be shown
        /// </summary>
        public bool ShowNotInstalledGames { get; set; }

        /// <summary>
        /// Indicates if the application should close when a game is launched
        /// </summary>
        public bool CloseAppOnGameLaunch { get; set; }

        /// <summary>
        /// Indicates if configuration dialogs should close upon saving
        /// </summary>
        public bool CloseConfigOnSave { get; set; }

        /// <summary>
        /// The backup directory path
        /// </summary>
        public FileSystemPath BackupLocation
        {
            get => _backupLocation;
            set
            {
                if (value == _backupLocation)
                    return;

                var oldValue = _backupLocation;

                _backupLocation = value;

                if (!oldValue.DirectoryExists)
                {
                    RCF.Logger.LogInformationSource("The backup location has been changed, but the previous directory does not exist");
                    return;
                }

                RCF.Logger.LogInformationSource("The backup location has been changed and old backups are being moved...");

                _ = RCFRCP.App.MoveBackupsAsync(oldValue, value);
            }
        }

        /// <summary>
        /// Indicates if progress should be shown on the task bar
        /// </summary>
        public bool ShowProgressOnTaskBar { get; set; }

        /// <summary>
        /// The minimum exception level to display
        /// </summary>
        public ExceptionLevel DisplayExceptionLevel { get; set; }

        /// <summary>
        /// Indicates if the Rayman Raving Rabbids save data is in the install directory
        /// </summary>
        public bool RRRIsSaveDataInInstallDir { get; set; }

        /// <summary>
        /// Indicates if detailed game information should be shown in the game options
        /// </summary>
        public bool ShowDetailedGameInfo { get; set; }

        /// <summary>
        /// The current TPLS data if installed, otherwise null
        /// </summary>
        public TPLSData TPLSData { get; set; }

        /// <summary>
        /// Indicates if animations are enabled
        /// </summary>
        public bool EnableAnimations { get; set; }

        /// <summary>
        /// The current culture info
        /// </summary>
        [JsonIgnore]
        public CultureInfo CurrentCultureInfo
        {
            get => new CultureInfo(CurrentCulture);
            set => CurrentCulture = value?.Name ?? AppLanguages.DefaultCulture.Name;
        }

        /// <summary>
        /// The current culture in the application
        /// </summary>
        public string CurrentCulture
        {
            get => RCF.Data.CurrentCulture?.Name ?? AppLanguages.DefaultCulture.Name;
            set
            {
                // Lock the culture changing code
                lock (this)
                {
                    // Store the culture info
                    CultureInfo ci;

                    try
                    {
                        // Attempt to get the culture info
                        ci = CultureInfo.GetCultureInfo(value);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleUnexpected("Getting culture info from setter string value");
                        ci = AppLanguages.DefaultCulture;
                    }

                    // Set the UI culture
                    CultureManager.UICulture = ci;

                    // Set the resource culture
                    Resources.Culture = ci;

                    // Update the current thread cultures
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = ci;

                    // Set the framework culture
                    RCF.Data.CurrentCulture = ci;

                    RCF.Logger.LogInformationSource($"The current culture was set to {ci.EnglishName}");
                }
            }
        }

        /// <summary>
        /// Indicates if languages with incomplete translations should be shown
        /// </summary>
        public bool ShowIncompleteTranslations
        {
            get => _showIncompleteTranslations;
            set
            {
                _showIncompleteTranslations = value;
                AppLanguages.RefreshLanguages(value);
            }
        }

        #endregion
    }
}
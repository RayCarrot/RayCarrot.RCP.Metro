using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Infralution.Localization.Wpf;
using MahApps.Metro;
using Microsoft.Win32;
using Newtonsoft.Json;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;
using RayCarrot.UserData;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

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
            RefreshShowUnderInstalledProgramsAsyncLock = new AsyncLock();
        }

        #endregion

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
            LinkItemStyle = LinkItemStyles.List;
            ApplicationPath = Assembly.GetExecutingAssembly().Location;
            ForceUpdate = false;
            ShowUnderInstalledPrograms = false;
            PendingRegUninstallKeyRefresh = false;
            GetBetaUpdates = false;
            LinkListHorizontalAlignment = HorizontalAlignment.Left;
        }

        #endregion

        #region Private Fields

        private bool _darkMode;

        private FileSystemPath _backupLocation;

        private bool _showIncompleteTranslations;

        private LinkItemStyles _linkItemStyle;

        private bool _showUnderInstalledPrograms;

        #endregion

        #region Private Properties

        /// <summary>
        /// Async lock for <see cref="RefreshShowUnderInstalledProgramsAsync"/>
        /// </summary>
        [JsonIgnore]
        private AsyncLock RefreshShowUnderInstalledProgramsAsyncLock { get; }

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

        // TODO: Remove this and instead back up the latest modified file and restore to BOTH places
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
            set => RefreshCulture(value);
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

        /// <summary>
        /// The current link item style
        /// </summary>
        public LinkItemStyles LinkItemStyle
        {
            get => _linkItemStyle;
            set
            {
                // Get previous source
                var oldSource = GetStyleSource(LinkItemStyle);

                _linkItemStyle = value;

                // Remove old source
                foreach (ResourceDictionary resourceDictionary in Application.Current.Resources.MergedDictionaries)
                {
                    if (!String.Equals(resourceDictionary.Source?.ToString(), oldSource, StringComparison.OrdinalIgnoreCase))
                        continue;

                    Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                    break;
                }

                // Add new source
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri(GetStyleSource(LinkItemStyle))
                });
            }
        }

        /// <summary>
        /// The last recorded path of the application
        /// </summary>
        public FileSystemPath ApplicationPath { get; set; }

        /// <summary>
        /// Indicates if a manual update should be forced even if the installed version is the latest one
        /// </summary>
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// Indicates if the program should display under installed programs
        /// </summary>
        public bool ShowUnderInstalledPrograms
        {
            get => _showUnderInstalledPrograms;
            set
            {
                if (_showUnderInstalledPrograms == value)
                    return;

                RefreshShowUnderInstalledProgramsAsync(value, false).ContinueWith(x =>
                {
                    if (!x.Result)
                    {
                        // Revert the property change
                        _showUnderInstalledPrograms = !value;

                        // Notify UI
                        OnPropertyChanged(nameof(ShowUnderInstalledPrograms));
                    }
                });
            }
        }

        /// <summary>
        /// Indicates if a refresh of the Registry uninstall key is pending
        /// </summary>
        public bool PendingRegUninstallKeyRefresh { get; set; }

        /// <summary>
        /// Indicates if beta updates should be searched for through the updater
        /// </summary>
        public bool GetBetaUpdates { get; set; }

        /// <summary>
        /// The horizontal alignment for link items in list view
        /// </summary>
        public HorizontalAlignment LinkListHorizontalAlignment { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the current culture
        /// </summary>
        /// <param name="cultureInfo">The culture info to set to</param>
        public void RefreshCulture(string cultureInfo)
        {
            lock (this)
            {
                // Store the culture info
                CultureInfo ci;

                try
                {
                    // Attempt to get the culture info
                    ci = CultureInfo.GetCultureInfo(cultureInfo);
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

        /// <summary>
        /// Refreshes the <see cref="ShowUnderInstalledPrograms"/> property
        /// </summary>
        /// <param name="showUnderInstalledPrograms">Indicates if the program should display under installed programs</param>
        /// <param name="forceRefresh">Indicates if the Registry key should be force refreshed</param>
        /// <returns>True if the operation succeeded, otherwise false</returns>
        public async Task<bool> RefreshShowUnderInstalledProgramsAsync(bool showUnderInstalledPrograms, bool forceRefresh)
        {
            using (await RefreshShowUnderInstalledProgramsAsyncLock.LockAsync())
            {
                RCF.Logger.LogDebugSource("The program Registry key is being updated...");

                _showUnderInstalledPrograms = showUnderInstalledPrograms;

                var keyExists = RCFWinReg.RegistryManager.KeyExists(RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, CommonPaths.RegistryUninstallKeyName));

                if (!forceRefresh && showUnderInstalledPrograms && keyExists)
                {
                    RCF.Logger.LogDebugSource("The program Registry key does not need to be modified due to already existing");

                    return true;
                }

                if (!showUnderInstalledPrograms && !keyExists)
                {
                    RCF.Logger.LogDebugSource("The program Registry key does not need to be modified due to not existing");

                    return true;
                }

                try
                {
                    if (!RCFRCP.App.IsRunningAsAdmin)
                    {
                        await RCF.MessageUI.DisplayMessageAsync(Resources.UninstallRegKeyRequiresRefresh, MessageType.Warning);
                        return false;
                    }

                    RCF.Logger.LogInformationSource("The program Registry key is being modified...");

                    using (var parentKey = RCFWinReg.RegistryManager.GetKeyFromFullPath(CommonRegistryPaths.InstalledPrograms, RegistryView.Default, true))
                    {
                        if (showUnderInstalledPrograms)
                        {
                            using (var subKey = parentKey.CreateSubKey(CommonPaths.RegistryUninstallKeyName))
                            {
                                if (subKey == null)
                                    throw new Exception("The created Registry uninstall key for RCP was null");

                                subKey.SetValue("DisplayName", "Rayman Control Panel", RegistryValueKind.String);
                                subKey.SetValue("DisplayVersion", RCFRCP.App.CurrentVersion.ToString(), RegistryValueKind.String);
                                subKey.SetValue("Publisher", "RayCarrot", RegistryValueKind.String);
                                subKey.SetValue("HelpLink", CommonUrls.DiscordUrl, RegistryValueKind.String);
                                subKey.SetValue("DisplayIcon", ApplicationPath, RegistryValueKind.String);
                                subKey.SetValue("InstallLocation", ApplicationPath.Parent, RegistryValueKind.String);
                                subKey.SetValue("UninstallString", $"\"{CommonPaths.UninstallFilePath}\" \"{ApplicationPath}\"", RegistryValueKind.String);
                                subKey.SetValue("VersionMajor", RCFRCP.App.CurrentVersion.Major, RegistryValueKind.DWord);
                                subKey.SetValue("VersionMinor", RCFRCP.App.CurrentVersion.Minor, RegistryValueKind.DWord);

                                // Attempt to get application size
                                try
                                {
                                    subKey.SetValue("EstimatedSize", ApplicationPath.GetSize().KiloBytes, RegistryValueKind.DWord);
                                }
                                catch (Exception ex)
                                {
                                    ex.HandleUnexpected("Getting app size");
                                }

                                // Attempt to get install date
                                try
                                {
                                    DateTime modifiedDate = File.GetLastWriteTime(ApplicationPath);
                                    subKey.SetValue("InstallDate", modifiedDate.ToString("yyyyMMdd"), RegistryValueKind.String);
                                }
                                catch (Exception ex)
                                {
                                    ex.HandleUnexpected("Getting app creation time");
                                }

                                RCF.Logger.LogInformationSource("The program Registry key has been created");
                            }
                        }
                        else
                        {
                            parentKey.DeleteSubKey(CommonPaths.RegistryUninstallKeyName);

                            RCF.Logger.LogInformationSource("The program Registry key has been deleted");
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Updating program in Registry uninstall key");

                    await RCF.MessageUI.DisplayMessageAsync(Resources.Settings_ShowUnderInstalledPrograms_UpdateError, MessageType.Error);

                    return false;
                }
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the style source name for a link item style
        /// </summary>
        /// <param name="linkItemStye">The style to get the source for</param>
        /// <returns>The source</returns>
        private static string GetStyleSource(LinkItemStyles linkItemStye)
        {
            return $"{AppViewModel.ApplicationBasePath}/Styles/LinkItemStyles - {linkItemStye}.xaml";
        }

        #endregion
    }
}
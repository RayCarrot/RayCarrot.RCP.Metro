using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The application user data
    /// </summary>
    public class AppUserData : BaseViewModel
    {
        #region Public Methods

        /// <summary>
        /// Resets all values to their defaults
        /// </summary>
        public void Reset()
        {
            UserLevel = UserLevel.Advanced;
            LastVersion = new Version(0, 0, 0, 0);
            WindowState = null;
            DarkMode = true;
            SyncTheme = false;
            ShowActionComplete = true;
            AutoUpdate = true;
            ShowProgressOnTaskBar = true;
            DisplayExceptionLevel = ExceptionLevel.Critical;
            EnableAnimations = true;
            CurrentCulture = LocalizationManager.DefaultCulture.Name;
            ShowIncompleteTranslations = false;
            ApplicationPath = Assembly.GetEntryAssembly()?.Location;
            ForceUpdate = false;
            GetBetaUpdates = false;
            DisableDowngradeWarning = false;
            IsUpdateAvailable = false;
            Games = new Dictionary<Games, UserData_GameData>();
            DosBoxGames = new Dictionary<Games, UserData_DosBoxOptions>();
            IsFirstLaunch = true;
            DosBoxPath = FileSystemPath.EmptyPath;
            DosBoxConfig = FileSystemPath.EmptyPath;
            AutoLocateGames = true;
            ShowNotInstalledGames = true;
            CloseAppOnGameLaunch = false;
            CloseConfigOnSave = true;
            BackupLocation = Environment.SpecialFolder.MyDocuments.GetFolderPath();
            TPLSData = null;
            LinkItemStyle = UserData_LinkItemStyle.List;
            LinkListHorizontalAlignment = HorizontalAlignment.Left;
            CompressBackups = true;
            FiestaRunVersion = UserData_FiestaRunEdition.Default;
            EducationalDosBoxGames = null;
            RRR2LaunchMode = UserData_RRR2LaunchMode.AllGames;
            RabbidsGoHomeLaunchData = null;
            JumpListItemIDCollection = new List<string>();
            InstalledGames = new HashSet<Games>();
            CategorizeGames = true;
            ShownRabbidsActivityCenterLaunchMessage = false;
            Archive_GF_GenerateMipmaps = true;
            Archive_GF_UpdateTransparency = UserData_Archive_GF_TransparencyMode.PreserveFormat;
            BinarySerializationFileLogPath = FileSystemPath.EmptyPath;
            HandleDownloadsManually = false;
            Archive_GF_ForceGF8888Import = false;
            ArchiveExplorerSortOption = UserData_Archive_Sort.Default;
            Archive_BinaryEditorExe = FileSystemPath.EmptyPath;
            Archive_AssociatedPrograms = new Dictionary<string, FileSystemPath>();
            Mod_RRR_KeyboardButtonMapping = new Dictionary<int, Key>();
        }

        /// <summary>
        /// Verifies all values are valid and corrects them if not
        /// </summary>
        public void Verify()
        {
            LastVersion ??= new Version(0, 0, 0, 0);
            CurrentCulture ??= LocalizationManager.DefaultCulture.Name;
            Games ??= new Dictionary<Games, UserData_GameData>();
            DosBoxGames ??= new Dictionary<Games, UserData_DosBoxOptions>();
            JumpListItemIDCollection ??= new List<string>();
            InstalledGames ??= new HashSet<Games>();
            Archive_AssociatedPrograms ??= new Dictionary<string, FileSystemPath>();
            Mod_RRR_KeyboardButtonMapping ??= new Dictionary<int, Key>();
        }

        public void AddAssociatedProgram(FileExtension ext, FileSystemPath exePath)
        {
            Archive_AssociatedPrograms.Add(ext.FileExtensions, exePath);
            OnPropertyChanged(nameof(Archive_AssociatedPrograms));
        }

        public void RemoveAssociatedProgram(FileExtension ext)
        {
            Archive_AssociatedPrograms.Remove(ext.FileExtensions);
            OnPropertyChanged(nameof(Archive_AssociatedPrograms));
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current user level
        /// </summary>
        public UserLevel UserLevel
        {
            get => Services.InstanceData.CurrentUserLevel;
            set => Services.InstanceData.CurrentUserLevel = value;
        }

        /// <summary>
        /// The last used version of the program
        /// </summary>
        public Version LastVersion { get; set; }

        /// <summary>
        /// The previous Window session state
        /// </summary>
        public UserData_WindowSessionState WindowState { get; set; }

        /// <summary>
        /// Indicates if the dark mode is on
        /// </summary>
        public bool DarkMode { get; set; }

        /// <summary>
        /// Indicates if the theme should sync with the system theme
        /// </summary>
        public bool SyncTheme { get; set; }

        /// <summary>
        /// Indicates if action complete messages should be shown
        /// </summary>
        public bool ShowActionComplete { get; set; }

        /// <summary>
        /// Indicates if updates should automatically be check for
        /// </summary>
        public bool AutoUpdate { get; set; }

        /// <summary>
        /// Indicates if progress should be shown on the task bar
        /// </summary>
        public bool ShowProgressOnTaskBar { get; set; }

        /// <summary>
        /// The minimum exception level to display
        /// </summary>
        public ExceptionLevel DisplayExceptionLevel { get; set; }

        /// <summary>
        /// Indicates if animations are enabled
        /// </summary>
        public bool EnableAnimations { get; set; }

        /// <summary>
        /// The current culture in the application
        /// </summary>
        public string CurrentCulture
        {
            get => Services.InstanceData.CurrentCulture?.Name ?? LocalizationManager.DefaultCulture.Name;
            set => LocalizationManager.SetCulture(value);
        }

        /// <summary>
        /// Indicates if languages with incomplete translations should be shown
        /// </summary>
        public bool ShowIncompleteTranslations { get; set; }

        /// <summary>
        /// The last recorded path of the application
        /// </summary>
        public FileSystemPath ApplicationPath { get; set; }

        /// <summary>
        /// Indicates if a manual update should be forced even if the installed version is the latest one
        /// </summary>
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// Indicates if beta updates should be searched for through the updater
        /// </summary>
        public bool GetBetaUpdates { get; set; }

        /// <summary>
        /// Indicates if the downgrade warnings should be disabled
        /// </summary>
        public bool DisableDowngradeWarning { get; set; }

        /// <summary>
        /// Indicates if a new update is available
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        /// <summary>
        /// The saved games
        /// </summary>
        public Dictionary<Games, UserData_GameData> Games { get; set; }

        /// <summary>
        /// The DosBox games options
        /// </summary>
        public Dictionary<Games, UserData_DosBoxOptions> DosBoxGames { get; set; }

        /// <summary>
        /// Indicates if it is the first time the application launches
        /// </summary>
        public bool IsFirstLaunch { get; set; }

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
        public FileSystemPath BackupLocation { get; set; }

        /// <summary>
        /// The current TPLS data if installed, otherwise null
        /// </summary>
        public UserData_TPLSData TPLSData { get; set; }

        /// <summary>
        /// The current link item style
        /// </summary>
        public UserData_LinkItemStyle LinkItemStyle { get; set; }

        /// <summary>
        /// The horizontal alignment for link items in list view
        /// </summary>
        public HorizontalAlignment LinkListHorizontalAlignment { get; set; }

        /// <summary>
        /// Indicates if backups should be compressed
        /// </summary>
        public bool CompressBackups { get; set; }

        /// <summary>
        /// Indicates the current version of Rayman Fiesta Run
        /// </summary>
        public UserData_FiestaRunEdition FiestaRunVersion { get; set; }

        /// <summary>
        /// The saved educational DOSBox games
        /// </summary>
        public List<UserData_EducationalDosBoxGameData> EducationalDosBoxGames { get; set; }

        /// <summary>
        /// The launch mode to use for Rayman Raving Rabbids 2
        /// </summary>
        public UserData_RRR2LaunchMode RRR2LaunchMode { get; set; }

        /// <summary>
        /// The launch data for Rabbids Go Home
        /// </summary>
        public UserData_RabbidsGoHomeLaunchData RabbidsGoHomeLaunchData { get; set; }

        /// <summary>
        /// The collection of jump list item IDs
        /// </summary>
        public List<string> JumpListItemIDCollection { get; set; }

        /// <summary>
        /// The games which have been installed through the Rayman Control Panel
        /// </summary>
        public HashSet<Games> InstalledGames { get; set; }

        /// <summary>
        /// Indicates if the games should be categorized
        /// </summary>
        public bool CategorizeGames { get; set; }

        /// <summary>
        /// Indicates if the launch message for Rayman Raving Rabbids Activity Center has been shown
        /// </summary>
        public bool ShownRabbidsActivityCenterLaunchMessage { get; set; }

        /// <summary>
        /// Indicates if mipmaps should be generated when importing files for .gf files (if supported)
        /// </summary>
        public bool Archive_GF_GenerateMipmaps { get; set; }

        /// <summary>
        /// Indicates if the image format should be updated depending on if the imported file supports transparency for .gf files 
        /// </summary>
        public UserData_Archive_GF_TransparencyMode Archive_GF_UpdateTransparency { get; set; }

        /// <summary>
        /// The binary serialization logging file path
        /// </summary>
        public FileSystemPath BinarySerializationFileLogPath { get; set; }

        /// <summary>
        /// Indicates if downloads should be handled manually. This does not apply to application updates.
        /// </summary>
        public bool HandleDownloadsManually { get; set; }

        /// <summary>
        /// Indicates if an imported .gf file should be forced to 888 (no transparency) or 8888 (with transparency)
        /// </summary>
        public bool Archive_GF_ForceGF8888Import { get; set; }

        /// <summary>
        /// The sort option for the Archive Explorer
        /// </summary>
        public UserData_Archive_Sort ArchiveExplorerSortOption { get; set; }

        /// <summary>
        /// The executable to use when opening a binary file for editing
        /// </summary>
        public FileSystemPath Archive_BinaryEditorExe { get; set; }

        /// <summary>
        /// The executables to use, associated with their file extensions
        /// </summary>
        public Dictionary<string, FileSystemPath> Archive_AssociatedPrograms { get; set; }

        /// <summary>
        /// The saved button mapping for the Rayman Raving Rabbids memory mod
        /// </summary>
        public Dictionary<int, Key> Mod_RRR_KeyboardButtonMapping { get; set; }

        #endregion
    }
}
﻿using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using PropertyChanged;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The application user data
/// </summary>
[SuppressPropertyChangedWarnings] // Suppress warnings since we use the legacy private setter properties
public class AppUserData : BaseViewModel
{
    #region Public Methods

    /// <summary>
    /// Resets all values to their defaults
    /// </summary>
    public void Reset()
    {
        // App
        App_UserLevel = UserLevel.Advanced;
        App_LastVersion = new Version(0, 0, 0, 0);
        App_ShowActionComplete = true;
        App_CurrentCulture = LocalizationManager.DefaultCulture.Name;
        App_ShowIncompleteTranslations = false;
        App_ApplicationPath = Assembly.GetEntryAssembly()?.Location;
        App_IsFirstLaunch = true;
        App_CloseAppOnGameLaunch = false;
        App_CloseConfigOnSave = true;
        App_JumpListItemIDCollection = new List<string>();
        App_HandleDownloadsManually = false;
        App_DisableGameValidation = false;

        // UI
        UI_WindowState = null;
        UI_ShowProgressOnTaskBar = true;
        UI_EnableAnimations = true;
        UI_LinkItemStyle = UserData_LinkItemStyle.List;
        UI_LinkListHorizontalAlignment = HorizontalAlignment.Left;
        UI_CategorizeGames = true;
        UI_UseChildWindows = true;

        // Theme
        Theme_DarkMode = true;
        Theme_SyncTheme = false;

        // Game
        Game_Games = new Dictionary<Games, UserData_GameData>();
        Game_DosBoxGames = new Dictionary<Games, UserData_DosBoxOptions>();
        Game_AutoLocateGames = true;
        Game_ShowNotInstalledGames = true;
        Game_FiestaRunVersion = UserData_FiestaRunEdition.Default;
        Game_EducationalDosBoxGames = null;
        Game_RRR2LaunchMode = UserData_RRR2LaunchMode.AllGames;
        Game_RabbidsGoHomeLaunchData = null;
        Game_InstalledGames = new HashSet<Games>();
        Game_ShownRabbidsActivityCenterLaunchMessage = false;

        // Emulator
        Emu_DOSBox_Path = FileSystemPath.EmptyPath;
        Emu_DOSBox_ConfigPath = FileSystemPath.EmptyPath;

        // Utility
        Utility_TPLSData = null;

        // Mod
        Mod_RRR_KeyboardButtonMapping = new Dictionary<int, Key>();

        // Update
        Update_AutoUpdate = true;
        Update_ForceUpdate = false;
        Update_GetBetaUpdates = false;
        Update_DisableDowngradeWarning = false;
        Update_IsUpdateAvailable = false;

        // Backup
        Backup_BackupLocation = Environment.SpecialFolder.MyDocuments.GetFolderPath();
        Backup_CompressBackups = true;

        // Archive
        Archive_GF_GenerateMipmaps = true;
        Archive_GF_UpdateTransparency = UserData_Archive_GF_TransparencyMode.PreserveFormat;
        Archive_GF_ForceGF8888Import = false;
        Archive_ExplorerSortOption = UserData_Archive_Sort.Default;
        Archive_BinaryEditorExe = FileSystemPath.EmptyPath;
        Archive_AssociatedPrograms = new Dictionary<string, FileSystemPath>();

        // Binary
        Binary_BinarySerializationFileLogPath = FileSystemPath.EmptyPath;
    }

    /// <summary>
    /// Verifies all values are valid and corrects them if not
    /// </summary>
    public void Verify()
    {
        // App
        App_LastVersion ??= new Version(0, 0, 0, 0);
        App_CurrentCulture ??= LocalizationManager.DefaultCulture.Name;
        App_JumpListItemIDCollection ??= new List<string>();
            
        // Game
        Game_Games ??= new Dictionary<Games, UserData_GameData>();
        Game_DosBoxGames ??= new Dictionary<Games, UserData_DosBoxOptions>();
        Game_InstalledGames ??= new HashSet<Games>();
            
        // Mod
        Mod_RRR_KeyboardButtonMapping ??= new Dictionary<int, Key>();
            
        // Archive
        Archive_AssociatedPrograms ??= new Dictionary<string, FileSystemPath>();
    }

    #endregion

    #region App

    /// <summary>
    /// The current user level
    /// </summary>
    public UserLevel App_UserLevel
    {
        get => Services.InstanceData.CurrentUserLevel;
        set => Services.InstanceData.CurrentUserLevel = value;
    }

    /// <summary>
    /// The last used version of the program
    /// </summary>
    public Version App_LastVersion { get; set; }

    /// <summary>
    /// Indicates if action complete messages should be shown
    /// </summary>
    public bool App_ShowActionComplete { get; set; }

    /// <summary>
    /// The current culture in the application
    /// </summary>
    public string App_CurrentCulture
    {
        get => Services.InstanceData.CurrentCulture?.Name ?? LocalizationManager.DefaultCulture.Name;
        set => LocalizationManager.SetCulture(value);
    }

    /// <summary>
    /// Indicates if languages with incomplete translations should be shown
    /// </summary>
    public bool App_ShowIncompleteTranslations { get; set; }

    /// <summary>
    /// The last recorded path of the application
    /// </summary>
    public FileSystemPath App_ApplicationPath { get; set; }

    /// <summary>
    /// Indicates if it is the first time the application launches
    /// </summary>
    public bool App_IsFirstLaunch { get; set; }

    /// <summary>
    /// Indicates if the application should close when a game is launched
    /// </summary>
    public bool App_CloseAppOnGameLaunch { get; set; }

    /// <summary>
    /// Indicates if configuration dialogs should close upon saving
    /// </summary>
    public bool App_CloseConfigOnSave { get; set; }

    /// <summary>
    /// The collection of jump list item IDs
    /// </summary>
    public List<string> App_JumpListItemIDCollection { get; set; }

    /// <summary>
    /// Indicates if downloads should be handled manually. This does not apply to application updates.
    /// </summary>
    public bool App_HandleDownloadsManually { get; set; }

    /// <summary>
    /// Disables validating the game install location. This allow a game with any install location to be added.
    /// </summary>
    public bool App_DisableGameValidation { get; set; }

    #endregion

    #region UI

    /// <summary>
    /// The previous Window session state
    /// </summary>
    public UserData_WindowSessionState UI_WindowState { get; set; }

    /// <summary>
    /// Indicates if progress should be shown on the task bar
    /// </summary>
    public bool UI_ShowProgressOnTaskBar { get; set; }

    /// <summary>
    /// Indicates if animations are enabled
    /// </summary>
    public bool UI_EnableAnimations { get; set; }

    /// <summary>
    /// The current link item style
    /// </summary>
    public UserData_LinkItemStyle UI_LinkItemStyle { get; set; }

    /// <summary>
    /// The horizontal alignment for link items in list view
    /// </summary>
    public HorizontalAlignment UI_LinkListHorizontalAlignment { get; set; }

    /// <summary>
    /// Indicates if the games should be categorized
    /// </summary>
    public bool UI_CategorizeGames { get; set; }

    /// <summary>
    /// Indicates if dialogs should be shown as child windows whenever possible
    /// </summary>
    public bool UI_UseChildWindows { get; set; }

    #endregion

    #region Theme

    /// <summary>
    /// Indicates if the dark mode is on
    /// </summary>
    public bool Theme_DarkMode { get; set; }

    /// <summary>
    /// Indicates if the theme should sync with the system theme
    /// </summary>
    public bool Theme_SyncTheme { get; set; }

    #endregion

    #region Game

    /// <summary>
    /// The saved games
    /// </summary>
    public Dictionary<Games, UserData_GameData> Game_Games { get; set; }

    /// <summary>
    /// The DosBox games options
    /// </summary>
    public Dictionary<Games, UserData_DosBoxOptions> Game_DosBoxGames { get; set; }

    /// <summary>
    /// Indicates if games should be automatically located on startup
    /// </summary>
    public bool Game_AutoLocateGames { get; set; }

    /// <summary>
    /// Indicates if not installed games should be shown
    /// </summary>
    public bool Game_ShowNotInstalledGames { get; set; }

    /// <summary>
    /// Indicates the current version of Rayman Fiesta Run
    /// </summary>
    public UserData_FiestaRunEdition Game_FiestaRunVersion { get; set; }

    /// <summary>
    /// The saved educational DOSBox games
    /// </summary>
    public List<UserData_EducationalDosBoxGameData> Game_EducationalDosBoxGames { get; set; }

    /// <summary>
    /// The launch mode to use for Rayman Raving Rabbids 2
    /// </summary>
    public UserData_RRR2LaunchMode Game_RRR2LaunchMode { get; set; }

    /// <summary>
    /// The launch data for Rabbids Go Home
    /// </summary>
    public UserData_RabbidsGoHomeLaunchData Game_RabbidsGoHomeLaunchData { get; set; }

    /// <summary>
    /// The games which have been installed through the Rayman Control Panel
    /// </summary>
    public HashSet<Games> Game_InstalledGames { get; set; }

    /// <summary>
    /// Indicates if the launch message for Rayman Raving Rabbids Activity Center has been shown
    /// </summary>
    public bool Game_ShownRabbidsActivityCenterLaunchMessage { get; set; }

    #endregion

    #region Emulator

    /// <summary>
    /// The path for the DosBox file
    /// </summary>
    public string Emu_DOSBox_Path { get; set; }

    /// <summary>
    /// Optional path for the DosBox config file
    /// </summary>
    public string Emu_DOSBox_ConfigPath { get; set; }

    #endregion

    #region Utility

    /// <summary>
    /// The current TPLS data if installed, otherwise null
    /// </summary>
    public UserData_TPLSData Utility_TPLSData { get; set; }

    #endregion

    #region Mod

    /// <summary>
    /// The saved button mapping for the Rayman Raving Rabbids memory mod
    /// </summary>
    public Dictionary<int, Key> Mod_RRR_KeyboardButtonMapping { get; set; }

    #endregion

    #region Update

    /// <summary>
    /// Indicates if updates should automatically be check for
    /// </summary>
    public bool Update_AutoUpdate { get; set; }

    /// <summary>
    /// Indicates if a manual update should be forced even if the installed version is the latest one
    /// </summary>
    public bool Update_ForceUpdate { get; set; }

    /// <summary>
    /// Indicates if beta updates should be searched for through the updater
    /// </summary>
    public bool Update_GetBetaUpdates { get; set; }

    /// <summary>
    /// Indicates if the downgrade warnings should be disabled
    /// </summary>
    public bool Update_DisableDowngradeWarning { get; set; }

    /// <summary>
    /// Indicates if a new update is available
    /// </summary>
    public bool Update_IsUpdateAvailable { get; set; }

    #endregion

    #region Backup

    /// <summary>
    /// The backup directory path
    /// </summary>
    public FileSystemPath Backup_BackupLocation { get; set; }

    /// <summary>
    /// Indicates if backups should be compressed
    /// </summary>
    public bool Backup_CompressBackups { get; set; }

    #endregion

    #region Archive

    /// <summary>
    /// Indicates if mipmaps should be generated when importing files for .gf files (if supported)
    /// </summary>
    public bool Archive_GF_GenerateMipmaps { get; set; }

    /// <summary>
    /// Indicates if the image format should be updated depending on if the imported file supports transparency for .gf files 
    /// </summary>
    public UserData_Archive_GF_TransparencyMode Archive_GF_UpdateTransparency { get; set; }

    /// <summary>
    /// Indicates if an imported .gf file should be forced to 888 (no transparency) or 8888 (with transparency)
    /// </summary>
    public bool Archive_GF_ForceGF8888Import { get; set; }

    /// <summary>
    /// The sort option for the Archive Explorer
    /// </summary>
    public UserData_Archive_Sort Archive_ExplorerSortOption { get; set; }

    /// <summary>
    /// The executable to use when opening a binary file for editing
    /// </summary>
    public FileSystemPath Archive_BinaryEditorExe { get; set; }

    /// <summary>
    /// The executables to use, associated with their file extensions
    /// </summary>
    public Dictionary<string, FileSystemPath> Archive_AssociatedPrograms { get; set; }

    public void Archive_AddAssociatedProgram(FileExtension ext, FileSystemPath exePath)
    {
        Archive_AssociatedPrograms.Add(ext.FileExtensions, exePath);
        OnPropertyChanged(nameof(Archive_AssociatedPrograms));
    }

    public void Archive_RemoveAssociatedProgram(FileExtension ext)
    {
        Archive_AssociatedPrograms.Remove(ext.FileExtensions);
        OnPropertyChanged(nameof(Archive_AssociatedPrograms));
    }

    #endregion

    #region Binary

    /// <summary>
    /// The binary serialization logging file path
    /// </summary>
    public FileSystemPath Binary_BinarySerializationFileLogPath { get; set; }

    #endregion

    #region Legacy

    // In version 12.0 the property names were changed. In order to still deserialize the properties using their old names we
    // provide legacy set-only properties for them

#pragma warning disable IDE0051 // Remove unused private members
    [JsonProperty] private UserLevel UserLevel { set => App_UserLevel = value; }
    [JsonProperty] private Version LastVersion { set => App_LastVersion = value; }
    [JsonProperty] private UserData_WindowSessionState WindowState { set => UI_WindowState = value; }
    [JsonProperty] private bool DarkMode { set => Theme_DarkMode = value; }
    [JsonProperty] private bool SyncTheme { set => Theme_SyncTheme = value; }
    [JsonProperty] private bool ShowActionComplete { set => App_ShowActionComplete = value; }
    [JsonProperty] private bool AutoUpdate { set => Update_AutoUpdate = value; }
    [JsonProperty] private bool ShowProgressOnTaskBar { set => UI_ShowProgressOnTaskBar = value; }
    [JsonProperty] private bool EnableAnimations { set => UI_EnableAnimations = value; }
    [JsonProperty] private string CurrentCulture { set => App_CurrentCulture = value; }
    [JsonProperty] private bool ShowIncompleteTranslations { set => App_ShowIncompleteTranslations = value; }
    [JsonProperty] private FileSystemPath ApplicationPath { set => App_ApplicationPath = value; }
    [JsonProperty] private bool ForceUpdate { set => Update_ForceUpdate = value; }
    [JsonProperty] private bool GetBetaUpdates { set => Update_GetBetaUpdates = value; }
    [JsonProperty] private bool DisableDowngradeWarning { set => Update_DisableDowngradeWarning = value; }
    [JsonProperty] private bool IsUpdateAvailable { set => Update_IsUpdateAvailable = value; }
    [JsonProperty] private Dictionary<Games, UserData_GameData> Games { set => Game_Games = value; }
    [JsonProperty] private Dictionary<Games, UserData_DosBoxOptions> DosBoxGames { set => Game_DosBoxGames = value; }
    [JsonProperty] private bool IsFirstLaunch { set => App_IsFirstLaunch = value; }
    [JsonProperty] private string DosBoxPath { set => Emu_DOSBox_Path = value; }
    [JsonProperty] private string DosBoxConfig { set => Emu_DOSBox_ConfigPath = value; }
    [JsonProperty] private bool AutoLocateGames { set => Game_AutoLocateGames = value; }
    [JsonProperty] private bool ShowNotInstalledGames { set => Game_ShowNotInstalledGames = value; }
    [JsonProperty] private bool CloseAppOnGameLaunch { set => App_CloseAppOnGameLaunch = value; }
    [JsonProperty] private bool CloseConfigOnSave { set => App_CloseConfigOnSave = value; }
    [JsonProperty] private FileSystemPath BackupLocation { set => Backup_BackupLocation = value; }
    [JsonProperty] private UserData_TPLSData TPLSData { set => Utility_TPLSData = value; }
    [JsonProperty] private UserData_LinkItemStyle LinkItemStyle { set => UI_LinkItemStyle = value; }
    [JsonProperty] private HorizontalAlignment LinkListHorizontalAlignment { set => UI_LinkListHorizontalAlignment = value; }
    [JsonProperty] private bool CompressBackups { set => Backup_CompressBackups = value; }
    [JsonProperty] private UserData_FiestaRunEdition FiestaRunVersion { set => Game_FiestaRunVersion = value; }
    [JsonProperty] private List<UserData_EducationalDosBoxGameData> EducationalDosBoxGames { set => Game_EducationalDosBoxGames = value; }
    [JsonProperty] private UserData_RRR2LaunchMode RRR2LaunchMode { set => Game_RRR2LaunchMode = value; }
    [JsonProperty] private UserData_RabbidsGoHomeLaunchData RabbidsGoHomeLaunchData { set => Game_RabbidsGoHomeLaunchData = value; }
    [JsonProperty] private List<string> JumpListItemIDCollection { set => App_JumpListItemIDCollection = value; }
    [JsonProperty] private HashSet<Games> InstalledGames { set => Game_InstalledGames = value; }
    [JsonProperty] private bool CategorizeGames { set => UI_CategorizeGames = value; }
    [JsonProperty] private bool ShownRabbidsActivityCenterLaunchMessage { set => Game_ShownRabbidsActivityCenterLaunchMessage = value; }
    [JsonProperty] private FileSystemPath BinarySerializationFileLogPath { set => Binary_BinarySerializationFileLogPath = value; }
    [JsonProperty] private bool HandleDownloadsManually { set => App_HandleDownloadsManually = value; }
    [JsonProperty] private UserData_Archive_Sort ArchiveExplorerSortOption { set => Archive_ExplorerSortOption = value; }
#pragma warning restore IDE0051 // Remove unused private members

    #endregion
}
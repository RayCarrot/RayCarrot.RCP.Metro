#nullable disable
using Newtonsoft.Json;
using PropertyChanged;
using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro;

[SuppressPropertyChangedWarnings] // Suppress warnings since we use the legacy private setter properties
public partial class AppUserData : BaseViewModel
{
    // In version 12.0 the property names were changed. In order to still deserialize the properties using their old names we
    // provide legacy set-only properties for them

#pragma warning disable IDE0051 // Remove unused private members
    [JsonProperty] private UserLevel UserLevel { set => App_UserLevel = value; }
    [JsonProperty] private Version LastVersion { set => App_LastVersion = value; }
    [JsonProperty] private WindowSessionState WindowState { set => UI_WindowState = value; }
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
    [JsonProperty] private bool IsFirstLaunch { set => App_IsFirstLaunch = value; }
    [JsonProperty] private bool AutoLocateGames { set => Game_AutoLocateGames = value; }
    [JsonProperty] private bool CloseAppOnGameLaunch { set => App_CloseAppOnGameLaunch = value; }
    [JsonProperty] private FileSystemPath BackupLocation { set => Backup_BackupLocation = value; }
    [JsonProperty] private bool CompressBackups { set => Backup_CompressBackups = value; }
    [JsonProperty] private FileSystemPath BinarySerializationFileLogPath { set => Binary_BinarySerializationFileLogPath = value; }
    [JsonProperty] private bool HandleDownloadsManually { set => App_HandleDownloadsManually = value; }
    [JsonProperty] private ArchiveItemsSort ArchiveExplorerSortOption { set => Archive_ExplorerSortOption = value; }
#pragma warning restore IDE0051 // Remove unused private members
}
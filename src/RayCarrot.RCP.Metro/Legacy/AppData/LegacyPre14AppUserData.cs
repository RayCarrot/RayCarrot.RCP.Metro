using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Legacy.AppData;

/// <summary>
/// Legacy app user data from before version 14.0, used for data migration
/// </summary>
public class LegacyPre14AppUserData
{
    public Dictionary<string, GameData>? Game_Games { get; set; }
    public HashSet<string>? Game_InstalledGames { get; set; }
    public Dictionary<string, DosBoxOptions>? Game_DosBoxGames { get; set; }
    public RaymanRavingRabbids2LaunchMode Game_RRR2LaunchMode { get; set; }
    public List<EducationalDosBoxGameData>? Game_EducationalDosBoxGames { get; set; }
    public bool Game_ShownRabbidsActivityCenterLaunchMessage { get; set; }
    public FiestaRunEdition Game_FiestaRunVersion { get; set; }
    public string? Emu_DOSBox_Path { get; set; }
    public string? Emu_DOSBox_ConfigPath { get; set; }
    public _TPLSData? Utility_TPLSData { get; set; }
    public Dictionary<string, ProgramDataSource>? Backup_GameDataSources { get; set; }

    // There was previously an oversight where this didn't get saved, so no need to migrate
    //RabbidsGoHomeLaunchData Game_RabbidsGoHomeLaunchData, RabbidsGoHomeLaunchData

    // Can't migrate jump list due to too many changes made to it
    //List<string> App_JumpListItemIDCollection, JumpListItemIDCollection

    // In version 12.0 the property names were changed. In order to still deserialize the properties
    // using their old names we provide legacy set-only properties for them
#pragma warning disable IDE0051 // Remove unused private members
    [JsonProperty] private Dictionary<string, GameData> Games { set => Game_Games = value; }
    [JsonProperty] private HashSet<string> InstalledGames { set => Game_InstalledGames = value; }
    [JsonProperty] private Dictionary<string, DosBoxOptions> DosBoxGames { set => Game_DosBoxGames = value; }
    [JsonProperty] private RaymanRavingRabbids2LaunchMode RRR2LaunchMode { set => Game_RRR2LaunchMode = value; }
    [JsonProperty] private List<EducationalDosBoxGameData> EducationalDosBoxGames { set => Game_EducationalDosBoxGames = value; }
    [JsonProperty] private string DosBoxPath { set => Emu_DOSBox_Path = value; }
    [JsonProperty] private string DosBoxConfig { set => Emu_DOSBox_ConfigPath = value; }
    [JsonProperty] private bool ShownRabbidsActivityCenterLaunchMessage { set => Game_ShownRabbidsActivityCenterLaunchMessage = value; }
    [JsonProperty] private FiestaRunEdition FiestaRunVersion { set => Game_FiestaRunVersion = value; }
    [JsonProperty] private _TPLSData TPLSData { set => Utility_TPLSData = value; }
#pragma warning restore IDE0051 // Remove unused private members

    public class GameData
    {
        public GameType GameType { get; set; }
        public FileSystemPath InstallDirectory { get; set; }
        public GameLaunchMode LaunchMode { get; set; }
    }

    public class DosBoxOptions
    {
        public FileSystemPath MountPath { get; set; }
    }

    public class EducationalDosBoxGameData
    {
        public FileSystemPath InstallDir { get; set; }
        public string? LaunchName { get; set; }
        public string? ID { get; set; }
        public string? Name { get; set; }
        public string? LaunchMode { get; set; }
        public FileSystemPath MountPath { get; set; }
    }

    public class _TPLSData
    {
        public FileSystemPath InstallDir { get; set; }
        public TPLSRaymanVersion RaymanVersion { get; set; }
        public bool IsEnabled { get; set; }
    }

    public enum GameType { Win32, Steam, WinStore, DosBox, EducationalDosBox }
    public enum GameLaunchMode { AsInvoker, AsAdminOption, AsAdmin }
    public enum RaymanRavingRabbids2LaunchMode { AllGames, Orange, Red, Green, Blue }
    public enum FiestaRunEdition { Default, Preload, Win10 }
    public enum TPLSRaymanVersion { Auto, Ray_1_00, Ray_1_10, Ray_1_12_0, Ray_1_12_1, Ray_1_12_2, Ray_1_20, Ray_1_21, Ray_1_21_Chinese }
    public enum ProgramDataSource { Auto, Default, VirtualStore }
}
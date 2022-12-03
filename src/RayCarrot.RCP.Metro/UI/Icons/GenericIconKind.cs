namespace RayCarrot.RCP.Metro;

public enum GenericIconKind
{
    None,

    Games,
    Games_Rayman,
    Games_Rabbids,
    Games_Demos,
    Games_Other,
    Games_FanGames,

    // TODO-UPDATE: Rename and re-reorganize for new ui
    GameDisplay_Admin,
    GameDisplay_Archive,
    GameDisplay_Patcher,
    GameDisplay_Location,
    GameDisplay_Web,
    GameDisplay_Steam,
    GameDisplay_Microsoft,
    GameDisplay_Purchase,

    GameAction_Play,

    GamePanel_Progression,

    GameAdd_Locate,
    GameAdd_Find,
    GameAdd_DiscInstall,
    GameAdd_Download,

    GameOptions_General,
    GameOptions_Config,
    GameOptions_Emulator,
    GameOptions_Utilities,

    Window_ArchiveExplorer,
    Window_ArchiveCreator,
    Window_Patcher,
    Window_PatchCreator,
    Window_GameOptions,
    Window_Installer,
    Window_Downloader,
    Window_DriveSelection,
    Window_FileExtensionSelection,
    Window_GamesSelection,
    Window_GameTypeSelection, // TODO-14: Remove this
    Window_JumpListEdit,
    Window_DialogMessage,
    Window_StringInput,
    Window_ProgramSelection,
    Window_AppNews,

    Utilities_SyncTextureInfo,
    Utilities_Rayman1_CompleteSoundtrack,
    Utilities_Rayman1_TPLS,
    Utilities_Rayman3_DirectPlay,
    Utilities_RaymanDesigner_CreateConfig,
    Utilities_RaymanDesigner_ReplaceFiles,
    Utilities_RaymanLegends_DebugCommands,
    Utilities_RaymanLegends_UbiRay,
    Utilities_RaymanOrigins_DebugCommands,
    Utilities_RaymanOrigins_HQVideos,
    Utilities_RaymanOrigins_Update,
    Utilities_RaymanFiestaRun_SaveFix,
    Utilities_ArchiveExplorer,
    Utilities_Ray1Editor,
    Utilities_R1PasswordGenerator,
    Utilities_Converters,
    Utilities_Decoders,
    Utilities_Serializers,
    Utilities_PatchCreator,

    Mods_Mem,
    Mods_RRR,
}
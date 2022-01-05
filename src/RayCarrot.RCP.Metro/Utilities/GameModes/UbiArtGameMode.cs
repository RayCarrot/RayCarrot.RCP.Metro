using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

public enum UbiArtGameMode
{
    [UbiArtGameModeInfo("Rayman Origins (PC)", Game.RaymanOrigins, Platform.PC, Games.RaymanOrigins)]
    RaymanOrigins_PC,

    [UbiArtGameModeInfo("Rayman Origins (PS3)", Game.RaymanOrigins, Platform.PlayStation3)]
    RaymanOrigins_PS3,

    [UbiArtGameModeInfo("Rayman Origins (Xbox 360)", Game.RaymanOrigins, Platform.Xbox360)]
    RaymanOrigins_Xbox360,

    [UbiArtGameModeInfo("Rayman Origins (Wii)", Game.RaymanOrigins, Platform.Wii)]
    RaymanOrigins_Wii,

    [UbiArtGameModeInfo("Rayman Origins (PS Vita)", Game.RaymanOrigins, Platform.PSVita)]
    RaymanOrigins_PSVita,

    [UbiArtGameModeInfo("Rayman Origins (3DS)", Game.RaymanOrigins, Platform.Nintendo3DS)]
    RaymanOrigins_3DS,

    [UbiArtGameModeInfo("Rayman Jungle Run (PC)", Game.RaymanJungleRun, Platform.PC, Games.RaymanJungleRun)]
    RaymanJungleRun_PC,

    [UbiArtGameModeInfo("Rayman Jungle Run (Android)", Game.RaymanJungleRun, Platform.Android)]
    RaymanJungleRun_Android,

    [UbiArtGameModeInfo("Rayman Legends (PC)", Game.RaymanLegends, Platform.PC, Games.RaymanLegends)]
    RaymanLegends_PC,

    [UbiArtGameModeInfo("Rayman Legends (Xbox 360)", Game.RaymanLegends, Platform.Xbox360)]
    RaymanLegends_Xbox360,

    [UbiArtGameModeInfo("Rayman Legends (Wii U)", Game.RaymanLegends, Platform.WiiU)]
    RaymanLegends_WiiU,

    [UbiArtGameModeInfo("Rayman Legends (PS Vita)", Game.RaymanLegends, Platform.PSVita)]
    RaymanLegends_PSVita,

    [UbiArtGameModeInfo("Rayman Legends (PS4)", Game.RaymanLegends, Platform.PlayStation4)]
    RaymanLegends_PS4,

    [UbiArtGameModeInfo("Rayman Legends (Switch)", Game.RaymanLegends, Platform.NintendoSwitch)]
    RaymanLegends_Switch,

    [UbiArtGameModeInfo("Rayman Fiesta Run (PC)", Game.RaymanFiestaRun, Platform.PC, Games.RaymanFiestaRun)]
    RaymanFiestaRun_PC,

    [UbiArtGameModeInfo("Rayman Fiesta Run (Android)", Game.RaymanJungleRun, Platform.Android)]
    RaymanFiestaRun_Android,

    [UbiArtGameModeInfo("Rayman Adventures (Android)", Game.RaymanAdventures, Platform.Android)]
    RaymanAdventures_Android,

    [UbiArtGameModeInfo("Rayman Adventures (iOS)", Game.RaymanAdventures, Platform.iOS)]
    RaymanAdventures_iOS,

    [UbiArtGameModeInfo("Rayman Mini (Mac)", Game.RaymanMini, Platform.Mac)]
    RaymanMini_Mac,

    [UbiArtGameModeInfo("Child of Light (PC)", Game.ChildOfLight, Platform.PC)]
    ChildOfLight_PC,

    [UbiArtGameModeInfo("Child of Light (PS Vita)", Game.ChildOfLight, Platform.PSVita)]
    ChildOfLight_PSVita,

    [UbiArtGameModeInfo("Valiant Hearts (Android)", Game.ValiantHearts, Platform.Android)]
    ValiantHearts_Android,

    [UbiArtGameModeInfo("Just Dance 2017 (Wii U)", Game.JustDance2017, Platform.WiiU)]
    JustDance_2017_WiiU,

    [UbiArtGameModeInfo("Gravity Falls (3DS)", Game.GravityFalls, Platform.Nintendo3DS)]
    GravityFalls_3DS,
}
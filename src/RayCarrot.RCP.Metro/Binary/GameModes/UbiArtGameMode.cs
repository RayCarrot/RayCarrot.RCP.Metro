using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

public enum UbiArtGameMode
{
    [UbiArtGameModeInfo("Rayman Origins (PC)", BinarySerializer.UbiArt.Game.RaymanOrigins, Platform.PC)]
    RaymanOrigins_PC,

    [UbiArtGameModeInfo("Rayman Origins (PS3)", BinarySerializer.UbiArt.Game.RaymanOrigins, Platform.PlayStation3)]
    RaymanOrigins_PS3,

    [UbiArtGameModeInfo("Rayman Origins (Xbox 360)", BinarySerializer.UbiArt.Game.RaymanOrigins, Platform.Xbox360)]
    RaymanOrigins_Xbox360,

    [UbiArtGameModeInfo("Rayman Origins (Wii)", BinarySerializer.UbiArt.Game.RaymanOrigins, Platform.Wii)]
    RaymanOrigins_Wii,

    [UbiArtGameModeInfo("Rayman Origins (PS Vita)", BinarySerializer.UbiArt.Game.RaymanOrigins, Platform.PSVita)]
    RaymanOrigins_PSVita,

    [UbiArtGameModeInfo("Rayman Origins (3DS)", BinarySerializer.UbiArt.Game.RaymanOrigins, Platform.Nintendo3DS)]
    RaymanOrigins_3DS,

    [UbiArtGameModeInfo("Rayman Jungle Run (PC)", BinarySerializer.UbiArt.Game.RaymanJungleRun, Platform.PC)]
    RaymanJungleRun_PC,

    [UbiArtGameModeInfo("Rayman Jungle Run (Android)", BinarySerializer.UbiArt.Game.RaymanJungleRun, Platform.Android)]
    RaymanJungleRun_Android,

    [UbiArtGameModeInfo("Rayman Legends (PC)", BinarySerializer.UbiArt.Game.RaymanLegends, Platform.PC)]
    RaymanLegends_PC,

    [UbiArtGameModeInfo("Rayman Legends (PS3)", BinarySerializer.UbiArt.Game.RaymanLegends, Platform.PlayStation3)]
    RaymanLegends_PS3,

    [UbiArtGameModeInfo("Rayman Legends (Xbox 360)", BinarySerializer.UbiArt.Game.RaymanLegends, Platform.Xbox360)]
    RaymanLegends_Xbox360,

    [UbiArtGameModeInfo("Rayman Legends (Wii U)", BinarySerializer.UbiArt.Game.RaymanLegends, Platform.WiiU)]
    RaymanLegends_WiiU,

    [UbiArtGameModeInfo("Rayman Legends (PS Vita)", BinarySerializer.UbiArt.Game.RaymanLegends, Platform.PSVita)]
    RaymanLegends_PSVita,

    [UbiArtGameModeInfo("Rayman Legends (PS4)", BinarySerializer.UbiArt.Game.RaymanLegends, Platform.PlayStation4)]
    RaymanLegends_PS4,

    [UbiArtGameModeInfo("Rayman Legends (Xbox One)", BinarySerializer.UbiArt.Game.RaymanLegends, Platform.XboxOne)]
    RaymanLegends_XboxOne,

    [UbiArtGameModeInfo("Rayman Legends (Switch)", BinarySerializer.UbiArt.Game.RaymanLegends, Platform.NintendoSwitch)]
    RaymanLegends_Switch,

    [UbiArtGameModeInfo("Rayman Fiesta Run (PC)", BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC)]
    RaymanFiestaRun_PC,

    [UbiArtGameModeInfo("Rayman Fiesta Run (Android)", BinarySerializer.UbiArt.Game.RaymanJungleRun, Platform.Android)]
    RaymanFiestaRun_Android,

    [UbiArtGameModeInfo("Rayman Adventures (Android)", BinarySerializer.UbiArt.Game.RaymanAdventures, Platform.Android)]
    RaymanAdventures_Android,

    [UbiArtGameModeInfo("Rayman Adventures (iOS)", BinarySerializer.UbiArt.Game.RaymanAdventures, Platform.iOS)]
    RaymanAdventures_iOS,

    [UbiArtGameModeInfo("Rayman Mini (Mac)", BinarySerializer.UbiArt.Game.RaymanMini, Platform.Mac)]
    RaymanMini_Mac,

    [UbiArtGameModeInfo("Child of Light (PC)", BinarySerializer.UbiArt.Game.ChildOfLight, Platform.PC)]
    ChildOfLight_PC,

    [UbiArtGameModeInfo("Child of Light (PS Vita)", BinarySerializer.UbiArt.Game.ChildOfLight, Platform.PSVita)]
    ChildOfLight_PSVita,

    [UbiArtGameModeInfo("Valiant Hearts (Android)", BinarySerializer.UbiArt.Game.ValiantHearts, Platform.Android)]
    ValiantHearts_Android,

    [UbiArtGameModeInfo("Just Dance 2017 (Wii U)", BinarySerializer.UbiArt.Game.JustDance2017, Platform.WiiU)]
    JustDance_2017_WiiU,

    [UbiArtGameModeInfo("Gravity Falls (3DS)", BinarySerializer.UbiArt.Game.GravityFalls, Platform.Nintendo3DS)]
    GravityFalls_3DS,
}
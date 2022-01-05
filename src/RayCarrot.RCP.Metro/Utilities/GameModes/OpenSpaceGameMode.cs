using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public enum OpenSpaceGameMode
{
    [OpenSpaceGameModeInfo("Rayman 2 (PC)", EngineVersion.Rayman2, Platform.PC, Games.Rayman2)]
    Rayman2_PC,

    [OpenSpaceGameModeInfo("Rayman 2 (DreamCast)", EngineVersion.Rayman2, Platform.Dreamcast)]
    Rayman2_DC,

    [OpenSpaceGameModeInfo("Rayman 2 (iOS)", EngineVersion.Rayman2, Platform.iOS)]
    Rayman2_iOS,

    [OpenSpaceGameModeInfo("Rayman 2 (PlayStation 2)", EngineVersion.Rayman2Revolution, Platform.PlayStation2)]
    Rayman2_PS2,

    [OpenSpaceGameModeInfo("Rayman 2 (Nintendo 64)", EngineVersion.Rayman2, Platform.Nintendo64)]
    Rayman2_N64,

    [OpenSpaceGameModeInfo("Rayman 2 (Nintendo DS)", EngineVersion.Rayman2, Platform.NintendoDS)]
    Rayman2_DS,

    [OpenSpaceGameModeInfo("Rayman 2 (Nintendo 3DS)", EngineVersion.Rayman2, Platform.Nintendo3DS)]
    Rayman2_3DS,

    [OpenSpaceGameModeInfo("Rayman 2 Demo 1999/08/18 (PC)", EngineVersion.Rayman2Demo, Platform.PC, Games.Demo_Rayman2_1)]
    Rayman2_Demo1_PC,

    [OpenSpaceGameModeInfo("Rayman 2 Demo 1999/09/04 (PC)", EngineVersion.Rayman2Demo, Platform.PC, Games.Demo_Rayman2_2)]
    Rayman2_Demo2_PC,

    [OpenSpaceGameModeInfo("Rayman M (PC)", EngineVersion.RaymanM, Platform.PC, Games.RaymanM)]
    RaymanM_PC,

    [OpenSpaceGameModeInfo("Rayman Arena (PC)", EngineVersion.RaymanArena, Platform.PC, Games.RaymanArena)]
    RaymanArena_PC,

    [OpenSpaceGameModeInfo("Rayman Arena (GameCube)", EngineVersion.RaymanArena, Platform.NintendoGameCube)]
    RaymanArena_GC,

    [OpenSpaceGameModeInfo("Rayman 3 (PC)", EngineVersion.Rayman3, Platform.PC, Games.Rayman3)]
    Rayman3_PC,

    [OpenSpaceGameModeInfo("Rayman 3 (GameCube)", EngineVersion.Rayman3, Platform.NintendoGameCube)]
    Rayman3_GC,

    [OpenSpaceGameModeInfo("Rayman Raving Rabbids (Nintendo DS)", EngineVersion.RaymanRavingRabbids, Platform.NintendoDS)]
    RaymanRavingRabbids_DS,

    [OpenSpaceGameModeInfo("Tonic Trouble (PC)", EngineVersion.TonicTrouble, Platform.PC)]
    TonicTrouble_PC,

    [OpenSpaceGameModeInfo("Tonic Trouble Special Edition (PC)", EngineVersion.TonicTroubleSpecialEdition, Platform.PC)]
    TonicTrouble_SE_PC,

    [OpenSpaceGameModeInfo("Donald Duck: Quack Attack (PC)", EngineVersion.DonaldDuckQuackAttack, Platform.PC)]
    DonaldDuck_PC,

    [OpenSpaceGameModeInfo("Donald Duck: Quack Attack (DreamCast)", EngineVersion.DonaldDuckQuackAttack, Platform.Dreamcast)]
    DonaldDuck_DC,

    [OpenSpaceGameModeInfo("Donald Duck: Quack Attack (Nintendo 64)", EngineVersion.DonaldDuckQuackAttack, Platform.Nintendo64)]
    DonaldDuck_N64,

    [OpenSpaceGameModeInfo("Donald Duck: PK (GameCube)", EngineVersion.DonaldDuckPK, Platform.NintendoGameCube)]
    DonaldDuckPK_GC,

    [OpenSpaceGameModeInfo("Playmobil: Hype (PC)", EngineVersion.PlaymobilHype, Platform.PC)]
    PlaymobilHype_PC,

    [OpenSpaceGameModeInfo("Playmobil: Laura (PC)", EngineVersion.PlaymobilLaura, Platform.PC)]
    PlaymobilLaura_PC,

    [OpenSpaceGameModeInfo("Playmobil: Alex (PC)", EngineVersion.PlaymobilAlex, Platform.PC)]
    PlaymobilAlex_PC,

    [OpenSpaceGameModeInfo("Disney's Dinosaur (PC)", EngineVersion.Dinosaur, Platform.PC)]
    Dinosaur_PC,

    [OpenSpaceGameModeInfo("Largo Winch (PC)", EngineVersion.LargoWinch, Platform.PC)]
    LargoWinch_PC,
}
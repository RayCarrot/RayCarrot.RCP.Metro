using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public enum CPAGameMode
{
    [CPAGameModeInfo("Rayman 2 (PC)", EngineVersion.Rayman2, Platform.PC)]
    Rayman2_PC,

    [CPAGameModeInfo("Rayman 2 (DreamCast)", EngineVersion.Rayman2, Platform.Dreamcast)]
    Rayman2_DC,

    [CPAGameModeInfo("Rayman 2 (iOS)", EngineVersion.Rayman2, Platform.iOS)]
    Rayman2_iOS,

    [CPAGameModeInfo("Rayman 2 (PlayStation)", EngineVersion.Rayman2, Platform.PlayStation1)]
    Rayman2_PS1,

    [CPAGameModeInfo("Rayman 2 (PlayStation 2)", EngineVersion.Rayman2Revolution, Platform.PlayStation2)]
    Rayman2_PS2,

    [CPAGameModeInfo("Rayman 2 (Nintendo 64)", EngineVersion.Rayman2, Platform.Nintendo64)]
    Rayman2_N64,

    [CPAGameModeInfo("Rayman 2 (Nintendo DS)", EngineVersion.Rayman2, Platform.NintendoDS)]
    Rayman2_DS,

    [CPAGameModeInfo("Rayman 2 (Nintendo 3DS)", EngineVersion.Rayman2, Platform.Nintendo3DS)]
    Rayman2_3DS,

    [CPAGameModeInfo("Rayman 2 Demo 1999/08/18 (PC)", EngineVersion.Rayman2Demo, Platform.PC)]
    Rayman2_Demo1_PC,

    [CPAGameModeInfo("Rayman 2 Demo 1999/09/04 (PC)", EngineVersion.Rayman2Demo, Platform.PC)]
    Rayman2_Demo2_PC,

    [CPAGameModeInfo("Rayman M (PC)", EngineVersion.RaymanM, Platform.PC)]
    RaymanM_PC,

    [CPAGameModeInfo("Rayman Arena (PC)", EngineVersion.RaymanM, Platform.PC)]
    RaymanArena_PC,

    [CPAGameModeInfo("Rayman Arena (GameCube)", EngineVersion.RaymanArena, Platform.NintendoGameCube)]
    RaymanArena_GC,

    [CPAGameModeInfo("Rayman Rush (PlayStation)", EngineVersion.RaymanRush, Platform.PlayStation1)]
    RaymanRush_PS1,

    [CPAGameModeInfo("Rayman 3 (PC)", EngineVersion.Rayman3, Platform.PC)]
    Rayman3_PC,

    [CPAGameModeInfo("Rayman 3 (GameCube)", EngineVersion.Rayman3, Platform.NintendoGameCube)]
    Rayman3_GC,

    [CPAGameModeInfo("Rayman Raving Rabbids (Nintendo DS)", EngineVersion.RaymanRavingRabbids, Platform.NintendoDS)]
    RaymanRavingRabbids_DS,

    [CPAGameModeInfo("Tonic Trouble (PC)", EngineVersion.TonicTrouble, Platform.PC)]
    TonicTrouble_PC,

    [CPAGameModeInfo("Tonic Trouble Special Edition (PC)", EngineVersion.TonicTroubleSpecialEdition, Platform.PC)]
    TonicTrouble_SE_PC,

    [CPAGameModeInfo("Donald Duck: Quack Attack (PC)", EngineVersion.DonaldDuckQuackAttack, Platform.PC)]
    DonaldDuck_PC,

    [CPAGameModeInfo("Donald Duck: Quack Attack (DreamCast)", EngineVersion.DonaldDuckQuackAttack, Platform.Dreamcast)]
    DonaldDuck_DC,

    [CPAGameModeInfo("Donald Duck: Quack Attack (Nintendo 64)", EngineVersion.DonaldDuckQuackAttack, Platform.Nintendo64)]
    DonaldDuck_N64,

    [CPAGameModeInfo("Donald Duck: PK (GameCube)", EngineVersion.DonaldDuckPK, Platform.NintendoGameCube)]
    DonaldDuckPK_GC,

    [CPAGameModeInfo("Playmobil: Hype (PC)", EngineVersion.PlaymobilHype, Platform.PC)]
    PlaymobilHype_PC,

    [CPAGameModeInfo("Playmobil: Laura (PC)", EngineVersion.PlaymobilLaura, Platform.PC)]
    PlaymobilLaura_PC,

    [CPAGameModeInfo("Playmobil: Alex (PC)", EngineVersion.PlaymobilAlex, Platform.PC)]
    PlaymobilAlex_PC,

    [CPAGameModeInfo("Disney's Dinosaur (PC)", EngineVersion.Dinosaur, Platform.PC)]
    Dinosaur_PC,

    [CPAGameModeInfo("Largo Winch (PC)", EngineVersion.LargoWinch, Platform.PC)]
    LargoWinch_PC,
}
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

// TODO: Localize game mode names?
public enum Ray1GameMode
{
    [Ray1GameModeInfo("Rayman (PlayStation)", Ray1EngineVersion.PS1)]
    Rayman1_PS1,

    [Ray1GameModeInfo("Rayman (PC)", Ray1EngineVersion.PC)]
    Rayman1_PC,

    [Ray1GameModeInfo("Rayman Edutainment (PC)", Ray1EngineVersion.PC_Edu)]
    RaymanEducational_PC,

    [Ray1GameModeInfo("Rayman Designer (PC)", Ray1EngineVersion.PC_Kit)]
    RaymanDesigner_PC,

    [Ray1GameModeInfo("Rayman by his Fans (PC)", Ray1EngineVersion.PC_Fan)]
    RaymanByHisFans_PC,

    [Ray1GameModeInfo("Rayman 60 Levels (PC)", Ray1EngineVersion.PC_Fan)]
    Rayman60Levels_PC,

    [Ray1GameModeInfo("Rayman Advance (Game Boy Advance)", Ray1EngineVersion.GBA)]
    Rayman1_GBA,
}
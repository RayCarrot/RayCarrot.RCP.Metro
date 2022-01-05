using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro
{
    public enum Ray1GameMode
    {
        [Ray1GameModeInfo("Rayman 1 (PC)", Ray1EngineVersion.PC, Games.Rayman1)]
        Rayman1_PC,

        [Ray1GameModeInfo("Rayman Educational (PC)", Ray1EngineVersion.PC_Edu, Games.EducationalDos)]
        RaymanEducational_PC,

        [Ray1GameModeInfo("Rayman Designer (PC)", Ray1EngineVersion.PC_Kit, Games.RaymanDesigner)]
        RaymanDesigner_PC,

        [Ray1GameModeInfo("Rayman by his Fans (PC)", Ray1EngineVersion.PC_Fan, Games.RaymanByHisFans)]
        RaymanByHisFans_PC,

        [Ray1GameModeInfo("Rayman 60 Levels (PC)", Ray1EngineVersion.PC_Fan, Games.Rayman60Levels)]
        Rayman60Levels_PC,
    }
}
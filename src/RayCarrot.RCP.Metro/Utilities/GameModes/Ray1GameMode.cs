using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro
{
    public enum Ray1GameMode
    {
        [Ray1GameModeInfo("Rayman 1 (PC)", Ray1EngineVersion.PC, 
            typeof(GameDescriptor_Rayman1_MSDOS))]
        Rayman1_PC,

        [Ray1GameModeInfo("Rayman Educational (PC)", Ray1EngineVersion.PC_Edu, 
            typeof(GameDescriptor_EducationalDos_MSDOS))]
        RaymanEducational_PC,

        [Ray1GameModeInfo("Rayman Designer (PC)", Ray1EngineVersion.PC_Kit, 
            typeof(GameDescriptor_RaymanDesigner_MSDOS))]
        RaymanDesigner_PC,

        [Ray1GameModeInfo("Rayman by his Fans (PC)", Ray1EngineVersion.PC_Fan, 
            typeof(GameDescriptor_RaymanByHisFans_MSDOS))]
        RaymanByHisFans_PC,

        [Ray1GameModeInfo("Rayman 60 Levels (PC)", Ray1EngineVersion.PC_Fan, 
            typeof(GameDescriptor_Rayman60Levels_MSDOS))]
        Rayman60Levels_PC,
    }
}
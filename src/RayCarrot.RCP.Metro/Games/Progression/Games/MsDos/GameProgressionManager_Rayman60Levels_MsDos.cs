using System.IO;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman60Levels_MsDos : GameProgressionManager_RaymanDesigner_MsDos
{
    public GameProgressionManager_Rayman60Levels_MsDos(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    protected override int LevelsCount => 60;
    protected override Ray1EngineVersion EngineVersion => Ray1EngineVersion.PC_Fan;

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
        new(GameInstallation.InstallLocation.Directory + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0)
    };
}
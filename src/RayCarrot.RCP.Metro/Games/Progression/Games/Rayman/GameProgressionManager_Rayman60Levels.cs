using System.IO;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman60Levels : GameProgressionManager_RaymanDesigner
{
    public GameProgressionManager_Rayman60Levels(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    protected override int LevelsCount => 60;
    protected override Ray1EngineVersion EngineVersion => Ray1EngineVersion.PC_Fan;

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
        new(GameInstallation.InstallLocation + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0)
    };
}
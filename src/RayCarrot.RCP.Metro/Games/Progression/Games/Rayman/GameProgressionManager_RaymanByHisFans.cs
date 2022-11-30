using System.IO;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanByHisFans : GameProgressionManager_RaymanDesigner
{
    public GameProgressionManager_RaymanByHisFans(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    protected override int LevelsCount => 40;
    protected override Ray1EngineVersion EngineVersion => Ray1EngineVersion.PC_Fan;

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // NOTE: Due to a mistake where the .sct files were not included in previous backups we need to keep this version for legacy support
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.cfg", "1", 0),
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.cfg", "0", 1),
        new(GameInstallation.InstallLocation + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 1),
    };
}
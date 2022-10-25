using System.IO;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_Rayman60Levels : ProgressionGameViewModel_RaymanDesigner
{
    public ProgressionGameViewModel_Rayman60Levels(GameInstallation gameInstallation) : base(gameInstallation) { }

    protected override int LevelsCount => 60;
    protected override Ray1EngineVersion EngineVersion => Ray1EngineVersion.PC_Fan;

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
        new(GameInstallation.InstallLocation + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0)
    };
}
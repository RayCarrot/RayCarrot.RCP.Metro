using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_Rayman60Levels : ProgressionGameViewModel_RaymanDesigner
{
    public ProgressionGameViewModel_Rayman60Levels() : base(Games.Rayman60Levels) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0)
    };
}
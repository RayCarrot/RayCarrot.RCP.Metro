using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanByHisFans : ProgressionGameViewModel_RaymanDesigner
{
    public ProgressionGameViewModel_RaymanByHisFans() : base(Games.RaymanByHisFans) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // NOTE: Due to a mistake where the .sct files were not included in previous backups we need to keep this version for legacy support
        new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "1", 0),
        new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "0", 1),
        new GameBackups_Directory(Game.GetInstallDir() + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 1),
    };
}
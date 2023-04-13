using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanRavingRabbids2_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanRavingRabbids2_Win32(GameInstallation gameInstallation, string backupId) 
        : base(gameInstallation, backupId) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "RRR2", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };
}
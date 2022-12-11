using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanRavingRabbids2 : GameProgressionManager
{
    public GameProgressionManager_RaymanRavingRabbids2(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "RRR2", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };
}
using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_GloboxMoment : GameProgressionManager
{
    public GameProgressionManager_GloboxMoment(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications", SearchOption.TopDirectoryOnly, "globoxmoment.ini", "0", 0),
    };
}
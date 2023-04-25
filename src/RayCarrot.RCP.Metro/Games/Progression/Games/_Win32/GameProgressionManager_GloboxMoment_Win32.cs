using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_GloboxMoment_Win32 : GameProgressionManager
{
    public GameProgressionManager_GloboxMoment_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications", SearchOption.TopDirectoryOnly, "globoxmoment.ini", "0", 0),
    };
}
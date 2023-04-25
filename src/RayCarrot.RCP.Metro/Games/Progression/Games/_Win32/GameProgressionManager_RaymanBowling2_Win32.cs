using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanBowling2_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanBowling2_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman_Bowling_2_New_format", SearchOption.AllDirectories, "*", "0", 0),
    };
}
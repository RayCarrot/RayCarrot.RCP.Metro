using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_TonicTrouble_Win32 : GameProgressionManager
{
    public GameProgressionManager_TonicTrouble_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // NOTE: The special edition has different folder casing, but that doesn't really matter
        new(GameInstallation.InstallLocation.Directory + "gamedata" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
        new(GameInstallation.InstallLocation.Directory + "gamedata" + "OPTIONS", SearchOption.AllDirectories, "*", "1", 0)
    };
}
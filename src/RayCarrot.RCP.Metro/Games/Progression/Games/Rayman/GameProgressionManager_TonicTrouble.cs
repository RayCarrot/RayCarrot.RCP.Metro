using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_TonicTrouble : GameProgressionManager
{
    public GameProgressionManager_TonicTrouble(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // NOTE: The special edition has different folder casing, but that doesn't really matter
        new(GameInstallation.InstallLocation + "gamedata" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
        new(GameInstallation.InstallLocation + "gamedata" + "OPTIONS", SearchOption.AllDirectories, "*", "1", 0)
    };
}
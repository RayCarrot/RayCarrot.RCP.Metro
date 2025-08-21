using System.IO;
using Microsoft.WindowsAPICodePack.Shell;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_TonicTrouble_Win32 : GameProgressionManager
{
    public GameProgressionManager_TonicTrouble_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // Default location
        new(GameInstallation.InstallLocation.Directory + "gamedata" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
        new(GameInstallation.InstallLocation.Directory + "gamedata" + "Options", SearchOption.AllDirectories, "*", "1", 0),
        
        // Redirected location using Tonic Trouble Fix
        new(new FileSystemPath(KnownFolders.SavedGames.Path) + "Tonic Trouble" + "SaveGame", SearchOption.AllDirectories, "*", "2", 0),
        new(new FileSystemPath(KnownFolders.SavedGames.Path) + "Tonic Trouble" + "Options", SearchOption.AllDirectories, "*", "3", 0),
    };
}
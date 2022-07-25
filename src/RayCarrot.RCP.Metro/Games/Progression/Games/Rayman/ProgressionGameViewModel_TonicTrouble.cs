using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_TonicTrouble : ProgressionGameViewModel
{
    public ProgressionGameViewModel_TonicTrouble(Games game) : base(game) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // NOTE: The special edition has different folder casing, but that doesn't really matter
        new GameBackups_Directory(Game.GetInstallDir() + "gamedata" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "gamedata" + "OPTIONS", SearchOption.AllDirectories, "*", "1", 0)
    };
}
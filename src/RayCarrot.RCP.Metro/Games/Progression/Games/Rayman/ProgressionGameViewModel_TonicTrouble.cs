using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_TonicTrouble : ProgressionGameViewModel
{
    public ProgressionGameViewModel_TonicTrouble(GameInstallation gameInstallation) : base(gameInstallation) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        // NOTE: The special edition has different folder casing, but that doesn't really matter
        new(GameInstallation.InstallLocation + "gamedata" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
        new(GameInstallation.InstallLocation + "gamedata" + "OPTIONS", SearchOption.AllDirectories, "*", "1", 0)
    };
}
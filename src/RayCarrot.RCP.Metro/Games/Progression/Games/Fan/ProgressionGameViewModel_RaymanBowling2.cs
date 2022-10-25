using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanBowling2 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanBowling2(GameInstallation gameInstallation) : base(gameInstallation) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman_Bowling_2_New_format", SearchOption.AllDirectories, "*", "0", 0),
    };
}
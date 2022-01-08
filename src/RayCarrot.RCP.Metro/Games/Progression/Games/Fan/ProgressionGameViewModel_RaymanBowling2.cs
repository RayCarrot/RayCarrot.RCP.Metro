using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanBowling2 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanBowling2() : base(Games.RaymanBowling2) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman_Bowling_2_New_format", SearchOption.AllDirectories, "*", "0", 0),
    };
}
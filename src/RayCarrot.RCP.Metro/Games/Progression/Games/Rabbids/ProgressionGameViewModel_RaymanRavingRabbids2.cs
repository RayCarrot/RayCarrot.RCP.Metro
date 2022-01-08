using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanRavingRabbids2 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanRavingRabbids2() : base(Games.RaymanRavingRabbids2) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "RRR2", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };
}
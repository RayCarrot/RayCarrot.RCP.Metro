using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanRavingRabbids2 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanRavingRabbids2(GameInstallation gameInstallation) : base(gameInstallation) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "RRR2", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };
}
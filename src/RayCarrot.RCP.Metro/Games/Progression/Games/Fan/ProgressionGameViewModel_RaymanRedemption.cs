using System;
using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanRedemption : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanRedemption() : base(Games.RaymanRedemption) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "RaymanRedemption", SearchOption.AllDirectories, "*", "0", 0),
    };
}
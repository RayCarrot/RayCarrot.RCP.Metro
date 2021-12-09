using System;
using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanRavingRabbids : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanRavingRabbids() : base(Games.RaymanRavingRabbids) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
        new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "VirtualStore" + Game.GetInstallDir().RemoveRoot(), SearchOption.TopDirectoryOnly, "*.sav", "0", 0)
    };
}
using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_GloboxMoment : ProgressionGameViewModel
{
    public ProgressionGameViewModel_GloboxMoment(GameInstallation gameInstallation) : base(gameInstallation) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications", SearchOption.TopDirectoryOnly, "globoxmoment.ini", "0", 0),
    };
}
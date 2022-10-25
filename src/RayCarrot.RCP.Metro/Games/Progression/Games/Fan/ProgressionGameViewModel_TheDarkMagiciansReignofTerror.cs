using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_TheDarkMagiciansReignofTerror : ProgressionGameViewModel
{
    public ProgressionGameViewModel_TheDarkMagiciansReignofTerror(GameInstallation gameInstallation) : base(gameInstallation) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman__Dark_Magician_s_reign_of_terror_", SearchOption.AllDirectories, "*", "0", 0),
    };
}
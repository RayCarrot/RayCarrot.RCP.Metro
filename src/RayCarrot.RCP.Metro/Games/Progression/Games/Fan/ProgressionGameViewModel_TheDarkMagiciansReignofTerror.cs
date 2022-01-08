using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_TheDarkMagiciansReignofTerror : ProgressionGameViewModel
{
    public ProgressionGameViewModel_TheDarkMagiciansReignofTerror() : base(Games.TheDarkMagiciansReignofTerror) { }

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman__Dark_Magician_s_reign_of_terror_", SearchOption.AllDirectories, "*", "0", 0),
    };
}
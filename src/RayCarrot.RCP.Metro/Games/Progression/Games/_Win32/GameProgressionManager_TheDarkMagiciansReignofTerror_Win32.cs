using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_TheDarkMagiciansReignofTerror_Win32 : GameProgressionManager
{
    public GameProgressionManager_TheDarkMagiciansReignofTerror_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman__Dark_Magician_s_reign_of_terror_", SearchOption.AllDirectories, "*", "0", 0),
    };
}
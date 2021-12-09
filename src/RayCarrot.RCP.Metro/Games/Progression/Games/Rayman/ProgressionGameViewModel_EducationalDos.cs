using System.IO;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_EducationalDos : ProgressionGameViewModel
{
    public ProgressionGameViewModel_EducationalDos(UserData_EducationalDosBoxGameData gameData) : base(Games.EducationalDos, gameData.Name)
    {
        GameData = gameData;
    }

    public UserData_EducationalDosBoxGameData GameData { get; }

    protected override string BackupName => $"Educational Games - {GameData.LaunchMode}";
    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(GameData.InstallDir, SearchOption.TopDirectoryOnly, $"EDU{GameData.LaunchMode}??.SAV", "0", 0),
        new GameBackups_Directory(GameData.InstallDir, SearchOption.TopDirectoryOnly, $"EDU{GameData.LaunchMode}.CFG", "1", 0)
    };
}
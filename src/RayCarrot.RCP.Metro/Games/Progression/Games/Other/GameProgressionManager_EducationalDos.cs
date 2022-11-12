using System.IO;

namespace RayCarrot.RCP.Metro;

// TODO-14: Fix
//public class GameProgressionManager_EducationalDos : GameProgressionManager
//{
//    public GameProgressionManager_EducationalDos(GameInstallation gameInstallation, UserData_EducationalDosBoxGameData gameData) 
//        : base(gameInstallation, gameData.Name)
//    {
//        GameData = gameData;
//    }

//    public UserData_EducationalDosBoxGameData GameData { get; }

//    protected override string BackupName => $"Educational Games - {GameData.LaunchMode}";
//    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
//    {
//        new(GameData.InstallDir, SearchOption.TopDirectoryOnly, $"EDU{GameData.LaunchMode}??.SAV", "0", 0),
//        new(GameData.InstallDir, SearchOption.TopDirectoryOnly, $"EDU{GameData.LaunchMode}.CFG", "1", 0)
//    };
//}
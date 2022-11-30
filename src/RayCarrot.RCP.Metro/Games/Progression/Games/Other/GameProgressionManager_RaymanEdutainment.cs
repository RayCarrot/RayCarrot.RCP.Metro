using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanEdutainment : GameProgressionManager
{
    public GameProgressionManager_RaymanEdutainment(GameInstallation gameInstallation, string backupName, string primaryName, string secondaryName) 
        : base(gameInstallation, backupName)
    {
        PrimaryName = primaryName;
        SecondaryName = secondaryName;
    }

    public override string Name => SecondaryName;

    public string PrimaryName { get; }
    public string SecondaryName { get; }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, $"{PrimaryName}{SecondaryName}??.SAV", "0", 0),
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, $"{PrimaryName}{SecondaryName}.CFG", "1", 0)
    };
}
using System.IO;
using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanEdutainment_MsDos : GameProgressionManager
{
    public GameProgressionManager_RaymanEdutainment_MsDos(GameInstallation gameInstallation, string backupId, string primaryName, Ray1MsDosData.Version version) 
        : base(gameInstallation, backupId)
    {
        PrimaryName = primaryName;
        Version = version;
    }

    public override string Name => Version.DisplayName;

    public string PrimaryName { get; }
    public Ray1MsDosData.Version Version { get; }

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, $"{PrimaryName}{Version.Id}??.SAV", "0", 0),
        new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, $"{PrimaryName}{Version.Id}.CFG", "1", 0)
    };
}
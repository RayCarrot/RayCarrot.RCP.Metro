namespace RayCarrot.RCP.Metro.Games.Structure;

public class GameInstallationDirectoryPath : GameInstallationPath
{
    public GameInstallationDirectoryPath(string path, GameInstallationPathType type, bool required = false)
        : base(path, type, required) { }

    public override bool IsValid(FileSystemPath fullPath) => fullPath.DirectoryExists;
}
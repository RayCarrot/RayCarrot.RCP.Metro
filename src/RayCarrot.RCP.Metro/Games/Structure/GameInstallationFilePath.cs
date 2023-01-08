namespace RayCarrot.RCP.Metro.Games.Structure;

public class GameInstallationFilePath : GameInstallationPath
{
    public GameInstallationFilePath(string path, GameInstallationPathType type, bool required = false) 
        : base(path, type, required) { }

    public override bool IsValid(FileSystemPath fullPath) => fullPath.FileExists;
}
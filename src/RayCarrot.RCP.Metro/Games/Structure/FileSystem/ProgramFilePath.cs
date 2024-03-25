namespace RayCarrot.RCP.Metro.Games.Structure;

public class ProgramFilePath : ProgramPath
{
    public ProgramFilePath(string path, ProgramPathType type, bool required = false) 
        : base(path, type, required) { }

    public override bool IsValid(IFileSystemSource source, string fullPath) => source.FileExists(fullPath);
}
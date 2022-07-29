namespace RayCarrot.RCP.Metro;

public abstract class FileLaunchHandler
{
    public abstract bool DisableFullStartup { get; }

    public abstract bool IsValid(FileSystemPath filePath);
    public abstract void Invoke(FileSystemPath filePath);
}
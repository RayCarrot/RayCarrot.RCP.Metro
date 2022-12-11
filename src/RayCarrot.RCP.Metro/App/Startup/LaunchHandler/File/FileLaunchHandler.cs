namespace RayCarrot.RCP.Metro;

public abstract class FileLaunchHandler : LaunchArgHandler
{
    public static FileLaunchHandler[] Handlers => new FileLaunchHandler[]
    {
        new PatchFileLaunchHandler()
    };

    public static FileLaunchHandler? GetHandler(FileSystemPath filePath) => Handlers.FirstOrDefault(x => x.IsValid(filePath));

    public abstract bool IsValid(FileSystemPath filePath);
    public abstract void Invoke(FileSystemPath filePath, State state);
}
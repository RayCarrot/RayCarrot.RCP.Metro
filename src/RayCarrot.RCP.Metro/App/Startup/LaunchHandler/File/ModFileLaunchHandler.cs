namespace RayCarrot.RCP.Metro;

public class ModFileLaunchHandler : FileLaunchHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool DisableFullStartup => true;

    public override bool IsValid(FileSystemPath filePath)
    {
        // TODO-UPDATE: Check if the file has a valid archive extension for a mod. Allow .gp as well for legacy support.

        return false;
    }

    public override async void Invoke(FileSystemPath filePath, State state)
    {
        // Show the mod loader
        await Services.UI.ShowModLoaderAsync(new[] { filePath });
    }
}
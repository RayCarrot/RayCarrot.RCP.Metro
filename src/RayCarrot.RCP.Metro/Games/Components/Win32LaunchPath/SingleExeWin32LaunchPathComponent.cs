namespace RayCarrot.RCP.Metro;

public class SingleExeWin32LaunchPathComponent : Win32LaunchPathComponent
{
    public SingleExeWin32LaunchPathComponent() : base(GetLaunchPath) { }

    public static FileSystemPath GetLaunchPath(GameInstallation installation)
    {
        return installation.InstallLocation.FilePath;
    }
}
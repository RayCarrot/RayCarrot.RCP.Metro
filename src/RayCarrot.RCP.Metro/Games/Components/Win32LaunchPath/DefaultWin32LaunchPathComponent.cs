using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

public class DefaultWin32LaunchPathComponent : Win32LaunchPathComponent
{
    public DefaultWin32LaunchPathComponent() : base(GetLaunchPath) { }

    public static FileSystemPath GetLaunchPath(GameInstallation installation)
    {
        var structure = installation.GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>();
        return structure.GetAbsolutePath(installation, GameInstallationPathType.PrimaryExe);
    }
}
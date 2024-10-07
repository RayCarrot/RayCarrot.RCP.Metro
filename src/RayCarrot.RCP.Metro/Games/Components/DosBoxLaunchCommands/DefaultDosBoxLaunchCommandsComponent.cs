using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro.Games.Components;

public class DefaultDosBoxLaunchCommandsComponent : DosBoxLaunchCommandsComponent
{
    public override IReadOnlyList<string> GetLaunchCommands(string? gameLaunchArgs = null)
    {
        List<string> cmds = new();

        // Mount the disc
        FileSystemPath mountPath = GameInstallation.GetValue<FileSystemPath>(GameDataKey.Client_DosBox_MountPath);
        if (mountPath.Exists)
        {
            // The mounting differs if it's a physical disc vs. a disc image
            if (mountPath.IsDirectoryRoot)
                cmds.Add($"mount d {mountPath.FullPath} -t cdrom");
            else
                cmds.Add($"imgmount d '{mountPath.FullPath}' -t iso -fs iso");
        }

        // Mount the game install directory as the C drive
        cmds.Add($"MOUNT C '{GameInstallation.InstallLocation.Directory}'");

        // Navigate to the mounted game directory
        cmds.Add("C:");

        // Run the game
        string exeFileName = GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>().FileSystem.GetLocalPath(ProgramPathType.PrimaryExe);
        string? gameArgs = gameLaunchArgs ?? GameInstallation.GetComponent<LaunchArgumentsComponent>()?.CreateObject();
        string launchName = gameArgs == null ? exeFileName : $"{exeFileName} {gameArgs}";
        cmds.Add($"{launchName}");

        // Exit
        cmds.Add("exit");

        return cmds;
    }
}
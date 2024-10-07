using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Games.Tools;
using RayCarrot.RCP.Metro.Games.Tools.PerLevelSoundtrack;

namespace RayCarrot.RCP.Metro.Games.Components;

public class PerLevelSoundtrackDosBoxLaunchCommandsComponent : DefaultDosBoxLaunchCommandsComponent
{
    private bool UsePerLevelSoundtrack(InstallableTool installableTool)
    {
        return Services.InstallableTools.CheckIsInstalled(installableTool) &&
               GameInstallation.GetObject<PerLevelSoundtrackData>(GameDataKey.R1_PerLevelSoundtrackData) is { IsEnabled: true };
    }

    public override IReadOnlyList<string> GetLaunchCommands(string? gameLaunchArgs = null)
    {
        PerLevelSoundtrackInstallableTool installableTool = new();

        if (!UsePerLevelSoundtrack(installableTool))
            return base.GetLaunchCommands(gameLaunchArgs);

        List<string> cmds = new();

        // Mount the custom TPLS TSR disc image
        cmds.Add($"imgmount d '{installableTool.CueFilePath}' -t iso -fs iso");

        // Mount the game install directory as the C drive
        cmds.Add($"MOUNT C '{GameInstallation.InstallLocation.Directory}'");

        // TODO-UPDATE: Pass in args
        // Run the TPLS TSR
        cmds.Add(@"D:\TPLSTSR4.EXE");

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
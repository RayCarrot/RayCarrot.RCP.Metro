namespace RayCarrot.RCP.Metro.Games.Components;

public class FindRaymanForeverFilesOnGameAddedComponent : OnGameAddedComponent
{
    public FindRaymanForeverFilesOnGameAddedComponent() : base(FindRaymanForeverFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static void FindRaymanForeverFiles(GameInstallation gameInstallation)
    {
        // Get the parent directory to the install directory
        FileSystemPath foreverInstallDir = gameInstallation.InstallLocation.Directory.Parent;

        // Attempt to automatically locate the mount file (based on the Rayman Forever location)
        FileSystemPath[] mountFiles =
        {
            foreverInstallDir + "game.inst",
            foreverInstallDir + "Music\\game.inst",
            foreverInstallDir + "game.ins",
            foreverInstallDir + "Music\\game.ins",
        };

        FileSystemPath mountPath = mountFiles.FirstOrDefault(x => x.FileExists);

        if (mountPath.FileExists)
        {
            gameInstallation.SetValue(GameDataKey.Client_DosBox_MountPath, mountPath);
            Logger.Info("The mount path for {0} was automatically found", gameInstallation.FullId);
        }
    }
}
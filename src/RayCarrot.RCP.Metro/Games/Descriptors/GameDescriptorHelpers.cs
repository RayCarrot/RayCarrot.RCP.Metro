namespace RayCarrot.RCP.Metro;

/// <summary>
/// Common game descriptor helper methods
/// </summary>
public static class GameDescriptorHelpers
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void PostAddRaymanForever(GameInstallation gameInstallation)
    {
        // Get the parent directory to the install directory
        FileSystemPath foreverInstallDir = gameInstallation.InstallLocation.Parent;

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
            gameInstallation.SetValue(GameDataKey.Emu_DosBox_MountPath, mountPath);
            Logger.Info("The mount path for {0} was automatically found", gameInstallation.FullId);
        }

        // TODO-14: Restore this? If user has not added any DOSBox emulators then attempt to add one and assign to this game?
        //          Or probably don't need to assign since that should be automatic when added.
        //// Find DOSBox path if not already added
        //if (!File.Exists(Services.Data.Emu_DOSBox_Path))
        //{
        //    FileSystemPath dosBoxPath = foreverInstallDir + "DosBox" + "DOSBox.exe";

        //    if (dosBoxPath.FileExists)
        //        Services.Data.Emu_DOSBox_Path = dosBoxPath;
        //}

        //// Find DOSBox config path if not already added
        //if (!File.Exists(Services.Data.Emu_DOSBox_ConfigPath))
        //{
        //    FileSystemPath dosBoxConfigPath = foreverInstallDir + "dosboxRayman.conf";

        //    if (dosBoxConfigPath.FileExists)
        //        Services.Data.Emu_DOSBox_ConfigPath = dosBoxConfigPath;
        //}
    }
}
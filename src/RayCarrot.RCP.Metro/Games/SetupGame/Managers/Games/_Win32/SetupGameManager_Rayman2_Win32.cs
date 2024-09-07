using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameManager_Rayman2_Win32 : SetupGameManager
{
    public SetupGameManager_Rayman2_Win32(GameInstallation gameInstallation) : base(gameInstallation) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool? IsGOGVersion()
    {
        try
        {
            // Get the exe file path
            DirectoryProgramInstallationStructure programStructure = GameInstallation.GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>();
            FileSystemPath path = programStructure.FileSystem.GetAbsolutePath(GameInstallation, ProgramPathType.PrimaryExe);

            // Get the file size
            long exeSize = path.GetSize();

            // Verify size
            return exeSize == 1_468_928; // Ubisoft Connect version is 1474664 bytes instead
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if the installation is the GOG version");
            return null;
        }
    }

    public override IEnumerable<SetupGameAction> GetRecommendedActions()
    {
        if (IsGOGVersion() == true)
        {
            // TODO-LOC
            // Ray2Fix
            yield return new SetupGameModAction(
                header: "Install Ray2Fix",
                info: "Ray2Fix is a mod, primarily for the GOG version, by spitfirex86 that aims to simplify setting up the game. It also comes bundled with various tweaks, such as the ability to remap gamepad controls and proper widescreen support.",
                isComplete: (GameInstallation.InstallLocation.Directory + "R2FixCfg.exe").FileExists,
                gameBananaModId: 479402);
        }
    }
}
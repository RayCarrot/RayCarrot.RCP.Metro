using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class Ray2FixSetupGameAction : InstallModSetupGameAction
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override long GameBananaModId => 479402;
    protected override string ModId => "88080deb-5f26-4d08-b44f-b0b6b36d1e22";

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_Ray2Fix_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_Ray2Fix_Info));

    public override SetupGameActionType Type => SetupGameActionType.Recommended;

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        try
        {
            // Get the exe file path
            DirectoryProgramInstallationStructure programStructure = gameInstallation.GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>();
            FileSystemPath path = programStructure.FileSystem.GetAbsolutePath(gameInstallation, ProgramPathType.PrimaryExe);

            // Get the file size
            long exeSize = path.GetSize();

            // Verify size
            return exeSize == 1_468_928; // Ubisoft Connect version is 1474664 bytes instead
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if the installation is the GOG version");
            return false;
        }
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        // Override checking if the mod is installed to instead check if the Ray2Fix config file exists. This allows it to be
        // marked as complete if installed manually or installed through another mod, such as the Galaxy Edition.
        return (gameInstallation.InstallLocation.Directory + "R2FixCfg.exe").FileExists;
    }
}
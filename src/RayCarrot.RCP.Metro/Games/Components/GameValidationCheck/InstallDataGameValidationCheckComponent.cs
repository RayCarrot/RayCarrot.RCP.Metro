using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class InstallDataGameValidationCheckComponent : GameValidationCheckComponent
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool IsValid()
    {
        if (GameInstallation.GetObject<RCPGameInstallData>(GameDataKey.RCP_GameInstallData) is { } installData)
        {
            // Make sure the install directory exists
            if (!installData.InstallDir.DirectoryExists)
            {
                Logger.Warn("The game {0} is invalid due to the install directory not existing", GameInstallation.FullId);
                return false;
            }

            // Make sure the install mode is valid
            if (!Enum.IsDefined(typeof(RCPGameInstallData.RCPInstallMode), installData.InstallMode))
            {
                Logger.Warn("The game {0} is invalid due to the install mode not being defined", GameInstallation.FullId);
                return false;
            }
        }

        return true;
    }
}
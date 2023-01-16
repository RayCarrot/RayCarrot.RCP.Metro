using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class Ray1MsDosGameDataGameValidationCheckComponent : GameValidationCheckComponent
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool IsValid()
    {
        Ray1MsDosData? data = GameInstallation.GetObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);

        // The data has to exist
        if (data == null)
        {
            Logger.Warn("The game {0} is invalid due to the Ray1 MS-DOS data not existing", GameInstallation.FullId);
            return false;
        }

        // There has to be at least one game mode available
        if (data.AvailableVersions == null! || data.AvailableVersions.Length == 0)
        {
            Logger.Warn("The game {0} is invalid due to there not being any available versions", GameInstallation.FullId);
            return false;
        }

        // The available game modes has to contain the selected one
        if (data.AvailableVersions.All(x => x.Id != data.SelectedVersion))
        {
            Logger.Warn("The game {0} is invalid due to the select version not being defined", GameInstallation.FullId);
            return false;
        }

        return true;
    }
}
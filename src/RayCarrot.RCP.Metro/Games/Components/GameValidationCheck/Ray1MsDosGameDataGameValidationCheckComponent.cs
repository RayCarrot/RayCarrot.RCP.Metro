using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class Ray1MsDosGameDataGameValidationCheckComponent : GameValidationCheckComponent
{
    public override bool IsValid()
    {
        Ray1MsDosData? data = GameInstallation.GetObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);

        // TODO-14: Log
        // The data has to exist
        if (data == null)
            return false;

        // There has to be at least one game mode available
        if (data.AvailableGameModes == null! || data.AvailableGameModes.Length == 0) 
            return false;

        // The available game modes has to contain the selected one
        if (!data.AvailableGameModes.Contains(data.SelectedGameMode))
            return false;

        return true;
    }
}
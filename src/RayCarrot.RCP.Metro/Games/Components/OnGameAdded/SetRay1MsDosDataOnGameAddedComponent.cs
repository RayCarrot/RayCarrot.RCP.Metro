using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class SetRay1MsDosDataOnGameAddedComponent : OnGameAddedComponent
{
    public SetRay1MsDosDataOnGameAddedComponent() : base(SetRay1MsDosDataAsync) { }

    private static async Task SetRay1MsDosDataAsync(GameInstallation gameInstallation)
    {
        Ray1MsDosData data = await Ray1MsDosData.CreateAsync(gameInstallation);
        gameInstallation.SetObject(GameDataKey.Ray1_MsDosData, data);
    }
}
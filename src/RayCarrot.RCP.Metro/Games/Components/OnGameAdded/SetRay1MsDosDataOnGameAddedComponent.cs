using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class SetRay1MsDosDataOnGameAddedComponent : OnGameAddedComponent
{
    public SetRay1MsDosDataOnGameAddedComponent() : base(SetRay1MsDosData) { }

    private static void SetRay1MsDosData(GameInstallation gameInstallation)
    {
        Ray1MsDosData data = Ray1MsDosData.Create(gameInstallation);
        gameInstallation.SetObject(GameDataKey.Ray1_MsDosData, data);
    }
}
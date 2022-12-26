namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class UtilityComponent : FactoryGameComponent<Utility>
{
    public UtilityComponent(Func<GameInstallation, Utility> objFactory) : base(objFactory) { }
}
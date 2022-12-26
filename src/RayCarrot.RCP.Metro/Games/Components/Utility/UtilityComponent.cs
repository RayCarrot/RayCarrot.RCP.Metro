namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponent(IsBase = true)]
public class UtilityComponent : FactoryGameComponent<Utility>
{
    public UtilityComponent(Func<GameInstallation, Utility> objFactory) : base(objFactory) { }
}
namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
[GameFeature(nameof(Resources.UtilitiesPageHeader), GenericIconKind.GameFeature_Utilities)]
public class UtilityComponent : FactoryGameComponent<Utility>
{
    public UtilityComponent(Func<GameInstallation, Utility> objFactory) : base(objFactory) { }
}
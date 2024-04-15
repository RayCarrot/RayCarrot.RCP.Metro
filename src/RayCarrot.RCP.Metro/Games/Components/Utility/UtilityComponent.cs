namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
[GameFeature(nameof(Resources.UtilitiesPageHeader), GenericIconKind.GameFeature_Utilities)]
public class UtilityComponent : FactoryGameComponent<Utility>
{
    public UtilityComponent(Func<GameInstallation, Utility> objFactory) : base(objFactory) { }
}
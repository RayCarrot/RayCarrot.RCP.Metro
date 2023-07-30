namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
[GameFeature(nameof(Resources.Progression_Header), GenericIconKind.GamePanel_Progression)]
public class ProgressionManagersComponent : FactoryGameComponent<IEnumerable<GameProgressionManager>>
{
    public ProgressionManagersComponent(Func<GameInstallation, GameProgressionManager> objFactory) : base(x => objFactory(x).Yield())
    { }
    
    public ProgressionManagersComponent(Func<GameInstallation, IEnumerable<GameProgressionManager>> objFactory) : base(objFactory) 
    { }
}
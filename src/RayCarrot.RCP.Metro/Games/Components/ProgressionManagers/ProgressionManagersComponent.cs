namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class ProgressionManagersComponent : FactoryGameComponent<IEnumerable<GameProgressionManager>>
{
    public ProgressionManagersComponent(Func<GameInstallation, GameProgressionManager> objFactory) : base(x => objFactory(x).Yield())
    { }
    
    public ProgressionManagersComponent(Func<GameInstallation, IEnumerable<GameProgressionManager>> objFactory) : base(objFactory) 
    { }
}
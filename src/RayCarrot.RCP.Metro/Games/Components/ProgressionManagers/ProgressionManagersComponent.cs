namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
[GameFeature(nameof(Resources.Progression_Header), GenericIconKind.GameFeature_Progression)]
public class ProgressionManagersComponent : FactoryGameComponent<IEnumerable<GameProgressionManager>>
{
    public ProgressionManagersComponent(Func<GameInstallation, GameProgressionManager> objFactory) : base(x => objFactory(x).Yield())
    { }
    
    public ProgressionManagersComponent(Func<GameInstallation, IEnumerable<GameProgressionManager>> objFactory) : base(objFactory) 
    { }

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<GamePanelComponent>(new ProgressionGamePanelComponent(this));
    }
}
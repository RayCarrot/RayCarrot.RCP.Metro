namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
[SingleInstanceGameComponent]
public abstract class BinaryGameModeComponent : GameComponent
{
    protected BinaryGameModeComponent(Enum gameMode)
    {
        GameMode = gameMode;
        GameModeAttribute = gameMode.GetAttribute<GameModeBaseAttribute>() 
                            ?? throw new Exception($"Game mode value {gameMode} is not valid");
    }

    public Enum GameMode { get; }
    public GameModeBaseAttribute GameModeAttribute { get; }

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new InitializeContextComponent((_, c) =>
        {
            object? settings = GameModeAttribute.GetSettingsObject();
            
            if (settings != null)
                c.AddSettings(settings, settings.GetType());
        }));
    }
}
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

    // TODO-UPDATE: Use this in other places
    public T GetRequiredSettings<T>() 
        where T : class => 
        GameModeAttribute.GetSettingsObject() as T 
        ?? throw new Exception($"The settings object provided by the corresponding game mode {GameMode} is not of the correct type");

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
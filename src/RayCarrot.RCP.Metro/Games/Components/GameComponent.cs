using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

// TODO: Find some way to integrate this with the dependency injection system

/// <summary>
/// The base class for a game component. After construction it is expected
/// that it has been initialized and the game installation can be accessed.
/// </summary>
public abstract class GameComponent
{
    private GameDescriptor? _gameDescriptor;
    private GameInstallation? _gameInstallation;

    public GameDescriptor GameDescriptor => _gameDescriptor ?? throw new Exception("The component has not been initialized");
    public GameInstallation GameInstallation => _gameInstallation ?? throw new Exception("The component has not been initialized");

    public GameClientInstallation GetRequiredGameClientInstallation() =>
        GameDescriptor.GetRequiredAttachedGameClient(GameInstallation);

    public virtual void RegisterComponents(IGameComponentBuilder builder) { }

    public void Initialize(GameInstallation gameInstallation)
    {
        if (gameInstallation == null) 
            throw new ArgumentNullException(nameof(gameInstallation));
        if (_gameDescriptor is not null)
            throw new InvalidOperationException("The component can't be initialized after it has already been initialized");

        _gameDescriptor = gameInstallation.GameDescriptor;
        _gameInstallation = gameInstallation;
    }
}
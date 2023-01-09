using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// Adds an action for initializing a <see cref="Context"/> being used
/// for this game installation. This is usually used to add game-specific
/// settings to the context.
/// </summary>
[BaseGameComponent]
public class InitializeContextComponent : GameComponent
{
    public InitializeContextComponent(Action<GameInstallation, Context> initContextAction)
    {
        _initContextAction = initContextAction;
    }

    private readonly Action<GameInstallation, Context> _initContextAction;

    public void InitContext(Context context) => _initContextAction(GameInstallation, context);
}
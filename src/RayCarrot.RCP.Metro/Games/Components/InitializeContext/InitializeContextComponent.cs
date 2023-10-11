using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// Adds an action for initializing a <see cref="Context"/> being used
/// for this game installation. This is usually used to add game-specific
/// settings to the context.
/// </summary>
[BaseGameComponent]
public class InitializeContextComponent : ActionGameComponent<Context>
{
    public InitializeContextComponent(Action<GameInstallation, Context> action) : base(action) { }
}
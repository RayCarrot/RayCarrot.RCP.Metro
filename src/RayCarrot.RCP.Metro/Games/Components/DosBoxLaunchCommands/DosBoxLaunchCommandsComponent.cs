namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
public abstract class DosBoxLaunchCommandsComponent : GameComponent
{
    public abstract IReadOnlyList<string> GetLaunchCommands(string? gameLaunchArgs = null);
}
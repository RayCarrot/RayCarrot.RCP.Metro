namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class OnGameRemovedComponent : AsyncActionGameComponent
{
    public OnGameRemovedComponent(Func<GameInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameRemovedComponent(Action<GameInstallation> action) : base(action) { }
}
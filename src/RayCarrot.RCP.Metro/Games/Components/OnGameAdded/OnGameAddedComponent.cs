namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class OnGameAddedComponent : AsyncActionGameComponent
{
    public OnGameAddedComponent(Func<GameInstallation, Task> asyncAction) : base(asyncAction) { }
    public OnGameAddedComponent(Action<GameInstallation> action) : base(action) { }
}